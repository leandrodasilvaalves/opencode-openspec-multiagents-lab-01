namespace Lab.Orders.Application.Abstractions;

public sealed record IdempotencyRecord(string Key, string PayloadHash, Guid OrderId, DateTimeOffset CreatedAt);

public interface IIdempotencyStore
{
    Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SaveAsync(IdempotencyRecord record, CancellationToken cancellationToken = default);
}
