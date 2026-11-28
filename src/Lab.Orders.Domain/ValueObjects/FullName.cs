using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.ValueObjects;

public class FullName : ValueObject<FullName>
{
    public string Value { get; }

    public FullName(string value)
    {
        DomainNullException.ThrowIfNull(value);

        Value = value.Trim();
        Validate();
    }

    protected override string ErrorCode() => "INVALID_FULL_NAME";

    protected override void Validate()
    {
        ClearErrors();

        var terms = Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (terms.Length < 2)
        {
            AddError("Full name is required and must contain at least two terms.");
        }
    }

    public override string ToString() => Value;

    public static bool TryParse(string value, out FullName result)
    {
        DomainNullException.ThrowIfNull(value);

        result = new FullName(value);
        return result.IsValid();
    }

    public override bool Equals(FullName? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj) => Equals(obj as FullName);

    public override int GetHashCode() =>
        (GetType().GetHashCode() * 907) + Value.GetHashCode();
}
