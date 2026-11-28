using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.ValueObjects;

public class Email : ValueObject<Email>
{
    public string Value { get; }

    public Email(string value)
    {
        DomainNullException.ThrowIfNull(value);

        Value = value.Trim().ToLowerInvariant();
        Validate();
    }

    protected override string ErrorCode() => "INVALID_EMAIL";

    protected override void Validate()
    {
        ClearErrors();

        if (Value.Any(char.IsWhiteSpace))
        {
            AddError("Email must not contain spaces.");
            return;
        }

        var atIndex = Value.IndexOf('@');
        if (atIndex < 0 || Value.IndexOf('@', atIndex + 1) >= 0)
        {
            AddError("Email must contain exactly one '@'.");
            return;
        }

        var localPart = Value[..atIndex];
        var domainPart = Value[(atIndex + 1)..];

        if (localPart.Length == 0 || domainPart.Length == 0)
        {
            AddError("Email must have a non-empty local part and domain.");
            return;
        }

        if (!domainPart.Contains('.'))
        {
            AddError("Email domain must contain a dot.");
        }
    }

    public override string ToString() => Value;

    public static bool TryParse(string value, out Email result)
    {
        DomainNullException.ThrowIfNull(value);

        result = new Email(value);
        return result.IsValid();
    }

    public override bool Equals(Email? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj) => Equals(obj as Email);

    public override int GetHashCode() =>
        (GetType().GetHashCode() * 907) + Value.GetHashCode();
}
