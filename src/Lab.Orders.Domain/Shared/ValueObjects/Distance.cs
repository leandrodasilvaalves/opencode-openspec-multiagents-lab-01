using Lab.Orders.Domain.Contracts;

namespace Lab.Orders.Domain.Shared.ValueObjects;

public class Distance : ValueObject<Distance>
{
    public decimal Kilometers { get; }
    public int IntegerKilometers { get; }

    public Distance(decimal kilometers)
    {
        Kilometers = Math.Round(kilometers, 1, MidpointRounding.AwayFromZero);
        IntegerKilometers = (int)Math.Round(kilometers, 0, MidpointRounding.AwayFromZero);
        ValidateThrow();
    }

    public static Distance FromKilometers(decimal kilometers) => new(kilometers);

    protected override string ErrorCode() => "DISTANCE";

    protected override void Validate()
    {
        if (Kilometers < 0)
            AddError("Distance cannot be negative.");
    }

    public override bool Equals(Distance? other) => other is not null && Kilometers.Equals(other.Kilometers);

    public override bool Equals(object? obj) => Equals(obj as Distance);

    public override int GetHashCode() => Kilometers.GetHashCode();

    public override string ToString() => Kilometers.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
}
