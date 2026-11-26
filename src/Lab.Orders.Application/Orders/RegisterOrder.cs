using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Application.Common;
using Lab.Orders.Domain.Delivery;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Orders;
using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Application.Orders;

public sealed record DiscountInput(DiscountType Type, decimal? Amount, decimal? Percentage);

public sealed record OrderItemInput(string? Sku, int Quantity, DiscountInput? Discount);

public sealed record RegisterOrderCommand(
    Guid CustomerId,
    IReadOnlyList<OrderItemInput>? Items,
    PaymentMethod? PaymentMethod,
    DiscountInput? Discount,
    decimal DistanceKm,
    string? IdempotencyKey);

public sealed record RegisterOrderResult(
    Guid OrderId,
    Guid CustomerId,
    decimal Subtotal,
    decimal OrderDiscount,
    decimal DeliveryFee,
    decimal Total,
    int RoundedDistanceKm,
    PaymentMethod PaymentMethod,
    int ItemCount);

public sealed class RegisterOrder
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IIdempotencyStore _idempotencyStore;

    public RegisterOrder(
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IIdempotencyStore idempotencyStore)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _idempotencyStore = idempotencyStore;
    }

    public async Task<Result<RegisterOrderResult>> ExecuteAsync(
        RegisterOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
            return Result<RegisterOrderResult>.Validation("IdempotencyKeyRequired", "IdempotencyKey is required.");

        var idempotencyKey = command.IdempotencyKey.Trim();
        var payloadHash = ComputePayloadHash(command);

        var existing = await _idempotencyStore.GetAsync(idempotencyKey, cancellationToken);

        if (existing is not null)
        {
            if (!string.Equals(existing.PayloadHash, payloadHash, StringComparison.Ordinal))
                return Result<RegisterOrderResult>.Conflict(
                    "IdempotencyKeyConflict",
                    "IdempotencyKey was already used with a different payload.");

            var stored = await _orderRepository.GetByIdAsync(existing.OrderId, cancellationToken);

            return stored is null
                ? Result<RegisterOrderResult>.NotFound("OrderNotFound", "Order for existing idempotency key was not found.")
                : Result<RegisterOrderResult>.Ok(Map(stored));
        }

        var validation = Validate(command);

        if (validation is not null)
            return validation;

        var customer = await _customerRepository.GetByIdAsync(command.CustomerId, cancellationToken);

        if (customer is null)
            return Result<RegisterOrderResult>.NotFound("CustomerNotFound", $"Customer '{command.CustomerId}' was not found.");

        var items = new List<OrderItem>();

        foreach (var input in command.Items!)
        {
            var sku = input.Sku!.Trim();
            var product = await _productRepository.GetBySkuAsync(sku, cancellationToken);

            if (product is null)
                return Result<RegisterOrderResult>.NotFound("ProductNotFound", $"Product SKU '{sku}' was not found.");

            Discount? itemDiscount;

            try
            {
                itemDiscount = MapDiscount(input.Discount);
            }
            catch (DomainException exception)
            {
                return Result<RegisterOrderResult>.Validation("InvalidDiscount", exception.Message);
            }

            try
            {
                items.Add(OrderItem.Create(sku, input.Quantity, product.BasePrice, itemDiscount, _ => true));
            }
            catch (DomainException exception)
            {
                return Result<RegisterOrderResult>.Validation("InvalidOrderItem", exception.Message);
            }
        }

        Order order;

        try
        {
            var orderDiscount = MapDiscount(command.Discount);
            var distance = new Distance(command.DistanceKm);

            order = Order.Create(
                Guid.NewGuid(),
                customer,
                items,
                command.PaymentMethod!.Value,
                orderDiscount,
                distance);
        }
        catch (DomainException exception)
        {
            return Result<RegisterOrderResult>.Validation("InvalidOrder", exception.Message);
        }

        await _orderRepository.AddAsync(order, cancellationToken);

        await _idempotencyStore.SaveAsync(
            new IdempotencyRecord(idempotencyKey, payloadHash, order.OrderId, DateTimeOffset.UtcNow),
            cancellationToken);

        return Result<RegisterOrderResult>.Ok(Map(order));
    }

    private static Result<RegisterOrderResult>? Validate(RegisterOrderCommand command)
    {
        if (command.CustomerId == Guid.Empty)
            return Result<RegisterOrderResult>.Validation("CustomerIdRequired", "CustomerId is required.");

        if (command.Items is null || command.Items.Count == 0)
            return Result<RegisterOrderResult>.Validation("ItemsRequired", "Order requires at least one item.");

        if (command.PaymentMethod is null || !Enum.IsDefined(command.PaymentMethod.Value))
            return Result<RegisterOrderResult>.Validation("PaymentMethodRequired", "A valid payment method is required.");

        if (command.DistanceKm < 0m)
            return Result<RegisterOrderResult>.Validation("InvalidDistance", "Distance cannot be negative.");

        if (command.DistanceKm > Distance.MaxKm)
            return Result<RegisterOrderResult>.Validation("DistanceTooFar", "Distance cannot exceed 100 km.");

        var hasItemDiscounts = command.Items.Any(i => i.Discount is not null);

        if (command.Discount is not null && hasItemDiscounts)
            return Result<RegisterOrderResult>.Validation(
                "ConflictingDiscounts",
                "Order discount cannot be combined with item discounts.");

        if (command.Discount is not null && !IsDiscountInputValid(command.Discount, out var orderError))
            return Result<RegisterOrderResult>.Validation("InvalidDiscount", orderError!);

        foreach (var item in command.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Sku))
                return Result<RegisterOrderResult>.Validation("SkuRequired", "OrderItem SKU is required.");

            if (item.Quantity <= 0)
                return Result<RegisterOrderResult>.Validation("InvalidQuantity", "OrderItem quantity must be greater than zero.");

            if (item.Discount is not null && !IsDiscountInputValid(item.Discount, out var itemError))
                return Result<RegisterOrderResult>.Validation("InvalidDiscount", itemError!);
        }

        return null;
    }

    private static bool IsDiscountInputValid(DiscountInput discount, out string? error)
    {
        error = null;

        if (!Enum.IsDefined(discount.Type))
        {
            error = "Unknown discount type.";
            return false;
        }

        switch (discount.Type)
        {
            case DiscountType.Amount:
                if (discount.Amount is null || discount.Percentage is not null)
                {
                    error = "Amount discount requires Amount and no Percentage.";
                    return false;
                }

                if (discount.Amount < 0m)
                {
                    error = "Discount amount cannot be negative.";
                    return false;
                }

                break;
            case DiscountType.Percentage:
                if (discount.Percentage is null || discount.Amount is not null)
                {
                    error = "Percentage discount requires Percentage and no Amount.";
                    return false;
                }

                if (discount.Percentage < 0m)
                {
                    error = "Discount percentage cannot be negative.";
                    return false;
                }

                break;
        }

        return true;
    }

    private static Discount? MapDiscount(DiscountInput? input)
    {
        if (input is null)
            return null;

        return input.Type switch
        {
            DiscountType.Amount => Discount.ByAmount(new Money(input.Amount!.Value)),
            DiscountType.Percentage => Discount.ByPercentage(new Percentage(input.Percentage!.Value)),
            _ => throw new DomainException("Unknown discount type.")
        };
    }

    private static RegisterOrderResult Map(Order order) => new(
        order.OrderId,
        order.CustomerId,
        order.Subtotal.Amount,
        order.OrderDiscount.Amount,
        order.DeliveryFee.Amount.Amount,
        order.Total.Amount,
        order.DistanceKm.RoundedKm,
        order.PaymentMethod,
        order.OrderItems.Count);

    private static string ComputePayloadHash(RegisterOrderCommand command)
    {
        var payload = new
        {
            command.CustomerId,
            Items = (command.Items ?? []).Select(i => new
            {
                Sku = i.Sku?.Trim(),
                i.Quantity,
                i.Discount
            }).ToList(),
            PaymentMethod = command.PaymentMethod?.ToString(),
            command.Discount,
            command.DistanceKm
        };

        var json = JsonSerializer.Serialize(payload);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
    }
}
