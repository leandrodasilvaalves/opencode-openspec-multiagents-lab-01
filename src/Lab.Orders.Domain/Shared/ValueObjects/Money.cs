using System.Globalization;
using Lab.Orders.Domain.Contracts;

namespace Lab.Orders.Domain.Shared.ValueObjects;

public class Money : ValueObject<Money>
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Amount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        ValidateThrow();
    }

    public static Money FromDecimal(decimal amount) => new(amount);

    public Money Add(Money other) => new(Amount + other.Amount);

    public Money Subtract(Money other) => new(Amount - other.Amount);

    public Money Multiply(decimal multiplier) => new(Amount * multiplier);

    public static Money operator +(Money left, Money right) => left.Add(right);

    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;

    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;

    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;

    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;

    protected override string ErrorCode() => "MONEY";

    protected override void Validate()
    {
        if (Amount < 0)
            AddError("Money amount cannot be negative.");
    }

    public override bool Equals(Money? other) => other is not null && Amount.Equals(other.Amount);

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => Amount.GetHashCode();

    public override string ToString() => Amount.ToString("F2", CultureInfo.InvariantCulture);
}
