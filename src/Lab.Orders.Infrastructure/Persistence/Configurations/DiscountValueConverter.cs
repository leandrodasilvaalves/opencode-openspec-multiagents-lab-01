using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Infrastructure.Persistence.Configurations;

public sealed class DiscountValueConverter : ValueConverter<Discount?, string?>
{
    public DiscountValueConverter() : base(
        discount => Serialize(discount),
        value => Deserialize(value))
    {
    }

    private static string? Serialize(Discount? discount)
    {
        if (discount is null)
            return null;

        return discount.Type switch
        {
            DiscountType.Amount => $"Amount:{discount.Amount.Amount.ToString(CultureInfo.InvariantCulture)}",
            DiscountType.Percentage => $"Percentage:{discount.Percentage.Value.ToString(CultureInfo.InvariantCulture)}",
            _ => null
        };
    }

    private static Discount? Deserialize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var parts = value.Split(':');
        if (parts.Length != 2)
            return null;

        if (!Enum.TryParse<DiscountType>(parts[0], out var type))
            return null;

        return type switch
        {
            DiscountType.Amount when decimal.TryParse(parts[1], CultureInfo.InvariantCulture, out var amount) =>
                Discount.ByAmount(new Money(amount)),
            DiscountType.Percentage when decimal.TryParse(parts[1], CultureInfo.InvariantCulture, out var percentage) =>
                Discount.ByPercentage(new Percentage(percentage)),
            _ => null
        };
    }
}
