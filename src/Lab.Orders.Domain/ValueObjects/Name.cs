using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.ValueObjects;

public class Name : ValueObject<Name>
{
    public string FirstName { get; }
    public string LastName { get; }
    public string FullName { get; }

    public Name(params string[] names)
    {
        FirstName = names[0];
        LastName = names[^1];
        FullName = names.Aggregate((agg, current) => $"{agg} {current}");
        Validate();
    }
    
    protected override string ErrorCode() => "INVALID_NAME";

    protected override void Validate()
    {
        ClearErrors();

        if (string.IsNullOrEmpty(FirstName))
        {
            AddError("Firstname is required.");
        }

        if (string.IsNullOrEmpty(LastName))
        {
            AddError("Last is required.");
        }
    }

    public override string ToString() => FullName;

    public static implicit operator Name(string value)
    {
        _ = TryParse(value, out var name);
        return name;
    }

    public static bool TryParse(string value, out Name name)
    {
        DomainNullException.ThrowIfNull(value);

        name = new Name(value.Split(" "));
        return name.IsValid();
    }

    public override bool Equals(Name? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return FullName.Equals(other.FullName, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj) => Equals(obj as Name);

    public override int GetHashCode()
    {
        return (GetType().GetHashCode() * 907) + FullName.GetHashCode();
    }
}
