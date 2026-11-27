using System.Globalization;
using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.Shared.ValueObjects;

public class Percentage : ValueObject<Percentage>
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        Value = Math.Round(value, 2, MidpointRounding.AwayFromZero);
        ValidateThrow();
    }

    public static Percentage FromDecimal(decimal value) => new(value);

    public decimal ApplyTo(decimal baseValue)
    {
        if (Value > 100)
        {
            throw new DomainRuleException(new[]
            {
                new Error("PERCENTAGE_OVER_100", "Percentage cannot exceed 100 when applied.")
            });
        }

        var result = baseValue * Value / 100m;
        return Math.Round(result, 2, MidpointRounding.AwayFromZero);
    }

    protected override string ErrorCode() => "PERCENTAGE";

    protected override void Validate()
    {
        if (Value < 0)
            AddError("Percentage cannot be negative.");
    }

    public override bool Equals(Percentage? other) => other is not null && Value.Equals(other.Value);

    public override bool Equals(object? obj) => Equals(obj as Percentage);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString("F2", CultureInfo.InvariantCulture);
}
