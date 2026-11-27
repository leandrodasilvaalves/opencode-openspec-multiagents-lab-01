using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Delivery;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Shared.ValueObjects;

namespace Lab.Orders.Domain.Orders;

public class Order : Entity<Guid>
{
    public Guid CustomerId { get; private set; }
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
    public PaymentMethod PaymentMethod { get; private set; }
    public Discount? Discount { get; private set; }
    public Distance DistanceKm { get; private set; }
    public DeliveryFee DeliveryFee { get; private set; }
    public Money Subtotal { get; private set; }
    public Money OrderDiscountAmount { get; private set; }
    public Money Total { get; private set; }

    private readonly List<OrderItem> _orderItems = [];

    protected Order() : base() { }

    public Order(Guid id, Guid customerId, IEnumerable<OrderItem> orderItems, PaymentMethod paymentMethod,
        Discount? discount, Distance distanceKm)
    {
        Id = id;
        CustomerId = customerId;
        PaymentMethod = paymentMethod;
        Discount = discount;
        DistanceKm = distanceKm;

        foreach (var item in orderItems)
        {
            _orderItems.Add(item);
        }

        ValidateThrow();

        CalculateTotals();

        if (Discount is not null && OrderDiscountAmount.Amount > Subtotal.Amount)
        {
            throw new DomainRuleException(new[]
            {
                new Error("ORDER_DISCOUNT_EXCEEDS_SUBTOTAL", "Order discount cannot exceed subtotal.")
            });
        }
    }

    protected override Guid GeneratedId() => Guid.NewGuid();

    protected override string ErrorCode() => "ORDER";

    protected override void ValidateRules()
    {
        if (CustomerId == Guid.Empty)
            AddError("Order must have a customer.");
        if (_orderItems.Count == 0)
            AddError("Order must have at least one item.");
        if (!Enum.IsDefined(typeof(PaymentMethod), PaymentMethod))
            AddError("Invalid payment method.");
        if (Discount is not null && _orderItems.Any(item => item.Discount is not null))
            AddError("Order-level discount and item-level discount are mutually exclusive.");
    }

    private void CalculateTotals()
    {
        var subtotal = _orderItems.Sum(item => item.FinalPrice.Amount);
        Subtotal = new Money(subtotal);

        var discountAmount = Discount is not null ? Discount.ApplyTo(Subtotal.Amount) : 0m;
        OrderDiscountAmount = new Money(discountAmount);

        var parcel = Math.Max(0, Subtotal.Amount - OrderDiscountAmount.Amount);
        DeliveryFee = new DeliveryFee(DistanceKm);
        Total = new Money(parcel + DeliveryFee.Amount.Amount);
    }
}
