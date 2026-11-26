using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Domain.Delivery;

public readonly record struct Distance
{
    public const decimal MaxKm = 100m;

    public decimal Km { get; }
    public int RoundedKm { get; }

    public Distance(decimal km)
    {
        if (km < 0m)
            throw new DomainException("Distance cannot be negative.");

        var roundedToFirstDecimal = decimal.Round(km, 1, MidpointRounding.AwayFromZero);
        var roundedKm = decimal.Round(roundedToFirstDecimal, 0, MidpointRounding.AwayFromZero);

        if (roundedKm > MaxKm)
            throw new DomainException("Distance cannot exceed 100 km.");

        Km = km;
        RoundedKm = (int)roundedKm;
    }
}
