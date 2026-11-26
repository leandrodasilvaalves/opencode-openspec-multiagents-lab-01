using System.Globalization;

namespace Lab.Orders.Domain.Shared;

public readonly record struct Money
{
    public const int Scale = 2;

    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount < 0m)
            throw new DomainException("Money amount cannot be negative.");

        Amount = Round(amount);
    }

    public static Money Zero => new(0m);

    public static decimal Round(decimal value) => decimal.Round(value, Scale, MidpointRounding.AwayFromZero);

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    public static Money operator -(Money left, Money right) => new(left.Amount - right.Amount);

    public static Money operator *(Money left, decimal factor) => new(left.Amount * factor);

    public static implicit operator decimal(Money money) => money.Amount;

    public override string ToString() => Amount.ToString("F2", CultureInfo.InvariantCulture);
}
