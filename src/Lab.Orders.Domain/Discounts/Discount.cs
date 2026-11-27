using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Shared.ValueObjects;

namespace Lab.Orders.Domain.Discounts;

public class Discount : ValueObject<Discount>
{
    public DiscountType Type { get; }
    public Money? AmountValue { get; }
    public Percentage? PercentValue { get; }

    private Discount(DiscountType type, Money? amount, Percentage? percent)
    {
        Type = type;
        AmountValue = amount;
        PercentValue = percent;
        ValidateThrow();
    }

    public static Discount OfAmount(Money value) => new(DiscountType.Amount, value, null);

    public static Discount OfPercentage(Percentage value) => new(DiscountType.Percentage, null, value);

    public decimal ApplyTo(decimal baseValue)
    {
        return Type switch
        {
            DiscountType.Amount => AmountValue!.Amount,
            DiscountType.Percentage => PercentValue!.ApplyTo(baseValue),
            _ => 0m
        };
    }

    protected override string ErrorCode() => "DISCOUNT";

    protected override void Validate()
    {
        if (Type == DiscountType.Amount && AmountValue is null)
            AddError("Amount discount requires a value.");
        if (Type == DiscountType.Percentage && PercentValue is null)
            AddError("Percentage discount requires a value.");
        if (AmountValue is not null && AmountValue.Amount < 0)
            AddError("Discount amount cannot be negative.");
    }

    public override bool Equals(Discount? other) =>
        other is not null &&
        Type == other.Type &&
        EqualityComparer<Money?>.Default.Equals(AmountValue, other.AmountValue) &&
        EqualityComparer<Percentage?>.Default.Equals(PercentValue, other.PercentValue);

    public override bool Equals(object? obj) => Equals(obj as Discount);

    public override int GetHashCode() =>
        (Type.GetHashCode() * 907) ^
        (AmountValue is not null ? AmountValue.GetHashCode() : 0) ^
        (PercentValue is not null ? PercentValue.GetHashCode() : 0);

    public override string ToString() =>
        Type == DiscountType.Amount
            ? $"{AmountValue!.Amount:C}"
            : $"{PercentValue!.Value}%";
}
