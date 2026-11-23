namespace Lab.Orders.Application;

/// <summary>
/// Interface for repository of Customers.
/// </summary>
public interface ICustomersRepository
{
    Task<Lab.Orders.Domain.Customer?> GetByIdAsync(Guid id);
    Task AddAsync(Lab.Orders.Domain.Customer customer);
    Task SaveChangesAsync();
}
