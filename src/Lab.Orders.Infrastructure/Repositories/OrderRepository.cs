using Microsoft.EntityFrameworkCore;
using Lab.Orders.Domain.Orders;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Infrastructure.Persistence;

namespace Lab.Orders.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        if (order.Customer is not null)
        {
            var customerEntry = _context.Entry(order.Customer);
            if (customerEntry.State == EntityState.Detached)
            {
                _context.Attach(order.Customer);
            }
        }

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }
}
