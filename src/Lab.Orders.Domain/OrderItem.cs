namespace Lab.Orders.Domain;

/// <summary>
/// Entidade da linha de um Order: preço unitário, quantidade e desconto por item.
/// </summary>
public sealed class OrderItem
{
    public decimal UnitPrice { get; }

    public int Quantity { get; }

    public decimal DiscountPercent { get; }

    /// <summary>
    /// Linha do pedido já com o desconto aplicado: UnitPrice * Quantity * (1 - DiscountPercent / 100).
    /// </summary>
    public decimal LineTotal => UnitPrice * Quantity * (1m - DiscountPercent / 100m);

    public OrderItem(decimal unitPrice, int quantity, decimal discountPercent = 0m)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade da linha deve ser maior que zero.");
        }

        if (unitPrice <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "O preço unitário da linha não pode ser não-positivo.");
        }

        if (discountPercent is < 0m or > 100m)
        {
            throw new ArgumentOutOfRangeException(nameof(discountPercent), "O desconto por item deve estar entre 0 e 100.");
        }

        UnitPrice = unitPrice;
        Quantity = quantity;
        DiscountPercent = discountPercent;
    }
}
