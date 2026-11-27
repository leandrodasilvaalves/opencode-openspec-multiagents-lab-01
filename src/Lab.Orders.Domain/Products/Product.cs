using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Shared.ValueObjects;

namespace Lab.Orders.Domain.Products;

public class Product : Entity<Guid>
{
    public string Sku { get; private set; }
    public string Description { get; private set; }
    public Money BasePrice { get; private set; }

    protected Product() : base() { }

    public Product(Guid id, string sku, string description, Money basePrice)
    {
        Id = id;
        Sku = sku;
        Description = description;
        BasePrice = basePrice;
        ValidateThrow();
    }

    protected override Guid GeneratedId() => Guid.NewGuid();

    protected override string ErrorCode() => "PRODUCT";

    protected override void ValidateRules()
    {
        if (string.IsNullOrWhiteSpace(Sku))
            AddError("Product SKU is required.");
        if (string.IsNullOrWhiteSpace(Description))
            AddError("Product description is required.");
        if (BasePrice is null)
            AddError("Product base price is required.");
        else if (BasePrice.Amount < 0)
            AddError("Product base price cannot be negative.");
    }
}
