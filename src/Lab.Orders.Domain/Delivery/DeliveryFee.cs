using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Domain.Delivery;

public readonly record struct DeliveryFee
{
    public static readonly Money BaseFee = new(12m);
    public static readonly Money PerKmFee = new(1m);
    public const int FreeKmThreshold = 15;
    public const int MaxKm = 100;

    public Money Amount { get; }

    private DeliveryFee(Money amount) => Amount = amount;

    public static DeliveryFee Calculate(Distance distance)
    {
        if (distance.RoundedKm > MaxKm)
            throw new DomainException("Distance cannot exceed 100 km.");

        var extraKm = Math.Max(0, distance.RoundedKm - FreeKmThreshold);
        var amount = BaseFee + PerKmFee * extraKm;

        return new DeliveryFee(amount);
    }

    public static implicit operator Money(DeliveryFee fee) => fee.Amount;

    public override string ToString() => Amount.ToString();
}
