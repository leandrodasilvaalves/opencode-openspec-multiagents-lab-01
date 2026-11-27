using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Entities;
using Lab.Orders.Domain.Enums;

namespace Lab.Orders.Domain.Events.Customers;

public class CustomerInactivated(Customer entity) : Event<Guid>(entity)
{
    public override string Key => "CUSTOMER_HAS_BEEN_INACTIVATED";

    public CustomerStatus Status { get; } = entity.Status;
    public DateTimeOffset UpdateAt { get; } = entity.UpdateAt;
}