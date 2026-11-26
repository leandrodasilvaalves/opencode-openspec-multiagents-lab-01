using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Domain.Discounts;

public sealed class Discount
{
    public DiscountType Type { get; }
    public Money Amount { get; }
    public Percentage Percentage { get; }

    private Discount(DiscountType type, Money amount, Percentage percentage)
    {
        Type = type;
        Amount = amount;
        Percentage = percentage;
    }

    public static Discount ByAmount(Money amount)
    {
        if (amount.Amount < 0m)
            throw new DomainException("Discount amount cannot be negative.");

        return new Discount(DiscountType.Amount, amount, Percentage.Zero);
    }

    public static Discount ByPercentage(Percentage percentage)
    {
        if (percentage.Value < 0m)
            throw new DomainException("Discount percentage cannot be negative.");

        return new Discount(DiscountType.Percentage, Money.Zero, percentage);
    }

    public Money CalculateDiscount(Money baseValue) =>
        Type == DiscountType.Amount ? Amount : Percentage.ApplyTo(baseValue);
}
