using Lab.Orders.Domain.Customers;
using Lab.Orders.Domain.Delivery;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Domain.Orders;

public sealed class Order
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public Customer Customer { get; private set; } = null!;
    public IReadOnlyList<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();
    public PaymentMethod PaymentMethod { get; private set; }
    public Discount? Discount { get; private set; }
    public Distance DistanceKm { get; private set; }
    public DeliveryFee DeliveryFee { get; private set; }
    public Money Subtotal { get; private set; } = Money.Zero;
    public Money OrderDiscount { get; private set; } = Money.Zero;
    public Money Total { get; private set; } = Money.Zero;

    private Order()
    {
    }

    private Order(
        Guid orderId,
        Customer customer,
        IReadOnlyList<OrderItem> orderItems,
        PaymentMethod paymentMethod,
        Discount? discount,
        Distance distanceKm,
        DeliveryFee deliveryFee,
        Money subtotal,
        Money orderDiscount,
        Money total)
    {
        OrderId = orderId;
        CustomerId = customer.CustomerId;
        Customer = customer;
        OrderItems = orderItems;
        PaymentMethod = paymentMethod;
        Discount = discount;
        DistanceKm = distanceKm;
        DeliveryFee = deliveryFee;
        Subtotal = subtotal;
        OrderDiscount = orderDiscount;
        Total = total;
    }

    public static Order Create(
        Guid orderId,
        Customer? customer,
        IEnumerable<OrderItem>? orderItems,
        PaymentMethod? paymentMethod,
        Discount? discount,
        Distance distanceKm,
        Func<Guid, bool>? customerExists = null)
    {
        if (orderId == Guid.Empty)
            throw new DomainException("OrderId is required.");

        if (customer is null)
            throw new DomainException("Order requires a customer.");

        if (customerExists is not null && !customerExists(customer.CustomerId))
            throw new DomainException("Customer not found.");

        var items = orderItems?.ToList() ?? new List<OrderItem>();

        if (items.Count == 0)
            throw new DomainException("Order requires at least one item.");

        if (items.Any(i => i is null))
            throw new DomainException("Order items cannot be null.");

        if (paymentMethod is null)
            throw new DomainException("Payment method is required.");

        if (!PaymentMethodParser.IsValid(paymentMethod.Value))
            throw new DomainException($"Invalid payment method. Accepted values: {string.Join(", ", Enum.GetNames<PaymentMethod>())}.");

        var subtotal = items.Aggregate(Money.Zero, (acc, item) => acc + item.LineTotal);

        var hasItemDiscounts = items.Any(i => i.Discount is not null);

        if (discount is not null && hasItemDiscounts)
            throw new DomainException("Order discount cannot be combined with item discounts; apply only individual discounts.");

        var orderDiscount = discount?.CalculateDiscount(subtotal) ?? Money.Zero;

        if (orderDiscount.Amount > subtotal.Amount)
            throw new DomainException("Order discount cannot make the total negative; discount cannot exceed subtotal.");

        var deliveryFee = DeliveryFee.Calculate(distanceKm);
        var discountedSubtotal = new Money(Math.Max(0m, subtotal.Amount - orderDiscount.Amount));
        var total = discountedSubtotal + deliveryFee.Amount;

        return new Order(
            orderId,
            customer,
            items,
            paymentMethod.Value,
            discount,
            distanceKm,
            deliveryFee,
            subtotal,
            orderDiscount,
            total);
    }
}
