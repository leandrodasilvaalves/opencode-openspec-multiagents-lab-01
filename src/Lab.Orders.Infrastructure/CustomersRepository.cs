using Lab.Orders.Application;
using Lab.Orders.Domain;

namespace Lab.Orders.Infrastructure;

/// <summary>
/// Implementação concreta do repositório de Clientes.
/// </summary>
public class CustomersRepository : ICustomersRepository
{
    private readonly List<Customer> _customers = new List<Customer>();

    public Task<Customer?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));
    }

    public Task AddAsync(Customer customer)
    {
        _customers.Add(customer);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        // Em um cenário real, isso persistiria no banco de dados.
        // Para este exemplo, é um no-op.
        return Task.CompletedTask;
    }
}
