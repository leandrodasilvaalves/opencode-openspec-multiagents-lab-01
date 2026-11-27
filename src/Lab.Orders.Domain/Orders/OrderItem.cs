using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Products;
using Lab.Orders.Domain.Shared.ValueObjects;

namespace Lab.Orders.Domain.Orders;

public class OrderItem : Entity<Guid>
{
    public string Sku { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Discount? Discount { get; private set; }
    public Money FinalPrice { get; private set; }

    protected OrderItem() : base() { }

    public OrderItem(Guid id, Product product, int quantity, Discount? discount)
    {
        Id = id;
        Sku = product.Sku;
        Quantity = quantity;
        UnitPrice = product.BasePrice;
        Discount = discount;
        FinalPrice = CalculateFinalPrice();
        ValidateThrow();
    }

    protected override Guid GeneratedId() => Guid.NewGuid();

    protected override string ErrorCode() => "ORDER_ITEM";

    protected override void ValidateRules()
    {
        if (string.IsNullOrWhiteSpace(Sku))
            AddError("Order item SKU is required.");
        if (Quantity <= 0)
            AddError("Order item quantity must be greater than zero.");
        if (Discount is not null)
        {
            var discountAmount = Discount.ApplyTo(UnitPrice.Amount);
            if (discountAmount > UnitPrice.Amount)
                AddError("Discount cannot exceed unit price.");
        }
    }

    private Money CalculateFinalPrice()
    {
        decimal discountAmount = 0m;
        if (Discount is not null)
        {
            discountAmount = Discount.ApplyTo(UnitPrice.Amount);
        }

        var subtotal = UnitPrice.Amount - discountAmount;
        if (subtotal < 0) subtotal = 0m;

        var total = subtotal * Quantity;
        return new Money(total);
    }
}
