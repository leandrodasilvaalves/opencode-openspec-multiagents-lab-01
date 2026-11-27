using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.Contracts;

public abstract class Entity<T> : Validatable
{
    public T Id { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdatedAt { get; protected set; }

    protected Entity()
    {
        Id = GeneratedId();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    protected abstract T GeneratedId();

    public override bool Equals(object? obj)
    {
        var compareTo = obj as Entity<T>;
        if (ReferenceEquals(this, compareTo)) return true;
        if (compareTo is null) return false;
        return Id!.Equals(compareTo.Id);
    }

    public override int GetHashCode()
    {
        return (GetType().GetHashCode() * 907) + Id!.GetHashCode();
    }

    protected override void Validate()
    {
        ClearErrors();
        ValidateRules();
        if (!IsValid())
        {
            throw new DomainRuleException(Errors);
        }
    }

    protected abstract void ValidateRules();

    public void Touch() => UpdatedAt = DateTimeOffset.UtcNow;
}

public abstract class Entity : Entity<Guid>
{
    protected override Guid GeneratedId() => Guid.NewGuid();
}
