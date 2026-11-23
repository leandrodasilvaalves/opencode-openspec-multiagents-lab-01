using Lab.Orders.Application;
using Lab.Orders.Domain;

namespace Lab.Orders.Infrastructure;

/// <summary>
/// Implementação concreta do repositório de Pedidos.
/// </summary>
public class OrdersRepository : IOrdersRepository
{
    private readonly List<Order> _orders = new List<Order>();

    public Task<Order?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
    }

    public Task AddAsync(Order order)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        // Em um cenário real, isso persistiria no banco de dados.
        // Para este exemplo, é um no-op.
        return Task.CompletedTask;
    }
}
