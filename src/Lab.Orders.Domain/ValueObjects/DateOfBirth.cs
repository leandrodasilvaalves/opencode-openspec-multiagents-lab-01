using System.Globalization;
using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.ValueObjects;

public class DateOfBirth : ValueObject<DateOfBirth>
{
    public DateOnly Value { get; }

    public int Age => CalculateAge(Value);

    public DateOfBirth(DateOnly value)
    {
        Value = value;
        Validate();
    }

    protected override string ErrorCode() => "INVALID_DATE_OF_BIRTH";

    protected override void Validate()
    {
        ClearErrors();

        if (Value == default)
        {
            AddError("Date of birth is required.");
            return;
        }

        if (Value > Today())
        {
            AddError("Date of birth cannot be in the future.");
            return;
        }

        if (Age < 18)
        {
            AddError("Customer must be at least 18 years old.");
        }
    }

    public override string ToString() => Value.ToString("yyyy-MM-dd");

    public static bool TryParse(string value, out DateOfBirth result)
    {
        DomainNullException.ThrowIfNull(value);

        if (!DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOfBirth))
        {
            result = null!;
            return false;
        }

        result = new DateOfBirth(dateOfBirth);
        return result.IsValid();
    }

    public override bool Equals(DateOfBirth? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj) => Equals(obj as DateOfBirth);

    public override int GetHashCode() =>
        (GetType().GetHashCode() * 907) + Value.GetHashCode();

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);

    private static int CalculateAge(DateOnly dateOfBirth)
    {
        var today = Today();
        var age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
