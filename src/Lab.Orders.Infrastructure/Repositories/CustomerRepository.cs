using Microsoft.EntityFrameworkCore;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Infrastructure.Persistence;

namespace Lab.Orders.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly OrdersDbContext _context;

    public CustomerRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
