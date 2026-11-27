using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.Contracts;

public abstract class Event<T>
{
    public T? AggregateId { get; protected set; }

    public long AggregateVersion { get; protected set; }

    public DateTimeOffset Timestamp { get; protected set; }

    public DateTimeOffset CreatedAt { get; protected set; }
    
    public DateTimeOffset UpdatedAt { get; protected set; }

    public abstract string Key { get; }

    protected Event()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    protected Event(Entity<T> entity) : this()
    {
        SetAggregateId(entity.Id);
        SetAggregateVersion(entity.Version);
        CreatedAt = entity.CreatedAt;
        UpdatedAt = entity.UpdateAt;
    }

    public void SetAggregateId(T aggregateId)
    {
        DomainNullException.ThrowIfNull(aggregateId);
        AggregateId = aggregateId;
    }

    public void SetAggregateVersion(long aggregateVersion)
    {
        DomainOutOfRangeException.ThrowIfLessThan(aggregateVersion, 0);
        AggregateVersion = aggregateVersion;
    }
}