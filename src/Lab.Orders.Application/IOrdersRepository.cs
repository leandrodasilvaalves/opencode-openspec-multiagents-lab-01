namespace Lab.Orders.Application;

/// <summary>
/// Interface for repository of Orders.
/// </summary>
public interface IOrdersRepository
{
    Task<Lab.Orders.Domain.Order?> GetByIdAsync(Guid id);
    Task AddAsync(Lab.Orders.Domain.Order order);
    Task SaveChangesAsync();
}
