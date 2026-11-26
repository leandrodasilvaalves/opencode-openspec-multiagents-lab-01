using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Domain.Products;

public sealed class Product
{
    public Guid ProductId { get; }
    public string Sku { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money BasePrice { get; private set; } = Money.Zero;

    private Product()
    {
    }

    private Product(Guid productId, string sku, string description, Money basePrice)
    {
        ProductId = productId;
        Sku = sku;
        Description = description;
        BasePrice = basePrice;
    }

    public static Product Create(Guid productId, string? sku, string? description, Money basePrice)
    {
        if (productId == Guid.Empty)
            throw new DomainException("ProductId is required.");

        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("Product SKU is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Product description is required.");

        if (basePrice.Amount < 0m)
            throw new DomainException("Product base price cannot be negative.");

        return new Product(productId, sku.Trim(), description.Trim(), basePrice);
    }
}
