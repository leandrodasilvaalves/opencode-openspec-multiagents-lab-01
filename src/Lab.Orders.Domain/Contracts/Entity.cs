using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.Contracts;

public abstract class Entity<T> : Validateble
{
    private readonly List<Event<T>> _events = [];

    public T Id { get; private set; }

    public DateTimeOffset CreatedAt { get; protected set; }

    public DateTimeOffset UpdateAt { get; protected set; }

    public IReadOnlyCollection<Event<T>> Events => _events;

    public long Version { get; protected set; }

    protected Entity()
    {
        Id = GeneratedId();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    protected abstract T GeneratedId();

    protected void UpVersion() => Version += 1;

    public void ClearEvents() => _events.Clear();

    protected void ApplyEvent(Event<T> @event)
    {
        ((dynamic)this).Apply((dynamic)@event);
        Version = @event.AggregateVersion;
        Id = @event.AggregateId!;
    }

    protected void AddEvent<TEvent>(TEvent @event) where TEvent : Event<T>
    {
        UpVersion();
        @event.SetAggregateVersion(Version);
        @event.SetAggregateId(Id);
        _events.Add(@event);
        ApplyEvent(@event);
    }


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
        ValidateRulers();
        if (IsInvalid())
        {
            throw new DomainRuleException(Errors);
        }
    }

    protected abstract void ValidateRulers();
}

public abstract class Entity : Entity<Guid>
{
    protected override Guid GeneratedId() => Guid.NewGuid();
}