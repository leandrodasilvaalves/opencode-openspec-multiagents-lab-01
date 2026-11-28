using System.Globalization;
using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.ValueObjects;

public class PhoneNumber : ValueObject<PhoneNumber>
{
    public string Value { get; }

    public PhoneNumber(string value)
    {
        DomainNullException.ThrowIfNull(value);

        Value = new string(value.Where(char.IsDigit).ToArray());
        Validate();
    }

    protected override string ErrorCode() => "INVALID_PHONE_NUMBER";

    protected override void Validate()
    {
        ClearErrors();

        if (Value.Length is not (10 or 11))
        {
            AddError("Phone number must contain 10 or 11 digits.");
            return;
        }

        var areaCode = int.Parse(Value[..2], CultureInfo.InvariantCulture);
        if (areaCode < 11)
        {
            AddError("Phone number must have an area code between 11 and 99.");
            return;
        }

        if (Value.Length == 11 && Value[2] != '9')
        {
            AddError("Mobile phone number must have the ninth digit equal to 9.");
        }
    }

    public override string ToString() => Value;

    public static bool TryParse(string value, out PhoneNumber result)
    {
        DomainNullException.ThrowIfNull(value);

        result = new PhoneNumber(value);
        return result.IsValid();
    }

    public override bool Equals(PhoneNumber? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj) => Equals(obj as PhoneNumber);

    public override int GetHashCode() =>
        (GetType().GetHashCode() * 907) + Value.GetHashCode();
}
