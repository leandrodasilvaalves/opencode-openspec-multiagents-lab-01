using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Entities;

namespace Lab.Orders.Domain.Events.Customers;

public class CustomerUpdate(Customer entity) : Event<Guid>(entity)
{
    public override string Key => "CUSTOMER_HAS_BEEN_UPDATED";
    public string Name { get; } = entity.Name!.FullName;
    public DateOnly DateOfBirth { get; } = entity.DateOfBirth;
}
