using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Domain.Orders;

public sealed class OrderItem
{
    public string Sku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public Discount? Discount { get; private set; }
    public Money BasePrice { get; private set; } = Money.Zero;
    public Money LineTotal { get; private set; } = Money.Zero;

    private OrderItem()
    {
    }

    private OrderItem(string sku, int quantity, Discount? discount, Money basePrice, Money lineTotal)
    {
        Sku = sku;
        Quantity = quantity;
        Discount = discount;
        BasePrice = basePrice;
        LineTotal = lineTotal;
    }

    public static OrderItem Create(
        string? sku,
        int quantity,
        Money basePrice,
        Discount? discount,
        Func<string, bool> skuExistsInCatalog)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new DomainException("OrderItem SKU is required.");

        var trimmedSku = sku.Trim();

        if (quantity <= 0)
            throw new DomainException("OrderItem quantity must be greater than zero.");

        if (basePrice.Amount < 0m)
            throw new DomainException("OrderItem base price cannot be negative.");

        if (skuExistsInCatalog is null)
            throw new DomainException("A SKU catalog validation hook is required.");

        if (!skuExistsInCatalog(trimmedSku))
            throw new DomainException($"SKU '{trimmedSku}' does not exist in the catalog.");

        var discountPerUnit = discount?.CalculateDiscount(basePrice) ?? Money.Zero;

        if (discountPerUnit.Amount > basePrice.Amount)
            throw new DomainException("OrderItem discount cannot exceed base price.");

        var effectiveUnitPrice = basePrice - discountPerUnit;
        var rawLineTotal = effectiveUnitPrice.Amount * quantity;
        var lineTotal = new Money(Math.Max(0m, Money.Round(rawLineTotal)));

        return new OrderItem(trimmedSku, quantity, discount, basePrice, lineTotal);
    }
}
