using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Shared.ValueObjects;

namespace Lab.Orders.Domain.Delivery;

public class DeliveryFee : ValueObject<DeliveryFee>
{
    public Money Amount { get; }
    public int DistanceKm { get; }

    public DeliveryFee(Distance distance)
    {
        DistanceKm = distance.IntegerKilometers;
        var fee = DistanceKm <= 15 ? 12m : 12m + (DistanceKm - 15) * 1m;
        Amount = new Money(fee);
        ValidateThrow();
    }

    public static DeliveryFee Calculate(Distance distance) => new(distance);

    protected override string ErrorCode() => "DELIVERY_FEE";

    protected override void Validate()
    {
        if (DistanceKm > 100)
            AddError("Distance exceeds the 100 km limit.");
    }

    public override bool Equals(DeliveryFee? other) => other is not null && Amount.Equals(other.Amount);

    public override bool Equals(object? obj) => Equals(obj as DeliveryFee);

    public override int GetHashCode() => Amount.GetHashCode();

    public override string ToString() => Amount.ToString();
}
