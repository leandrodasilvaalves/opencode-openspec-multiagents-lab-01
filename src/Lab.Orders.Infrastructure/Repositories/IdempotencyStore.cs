using Microsoft.EntityFrameworkCore;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Infrastructure.Persistence;

namespace Lab.Orders.Infrastructure.Repositories;

public sealed class IdempotencyStore : IIdempotencyStore
{
    private readonly OrdersDbContext _context;

    public IdempotencyStore(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<IdempotencyRecord?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.IdempotencyRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Key == key, cancellationToken);
    }

    public async Task SaveAsync(IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        await _context.IdempotencyRecords.AddAsync(record, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
