namespace Lab.Orders.Domain.Shared;

public readonly record struct Percentage
{
    public const decimal MaxValue = 100m;

    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value < 0m)
            throw new DomainException("Percentage cannot be negative.");

        Value = value;
    }

    public Money ApplyTo(Money baseValue)
    {
        if (Value > MaxValue)
            throw new DomainException("Percentage greater than 100 cannot be applied.");

        return new Money(baseValue.Amount * Value / 100m);
    }

    public static Percentage Zero => new(0m);
}
