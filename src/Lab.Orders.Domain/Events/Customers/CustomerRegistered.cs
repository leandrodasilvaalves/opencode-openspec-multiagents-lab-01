using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Entities;

namespace Lab.Orders.Domain.Events.Customers;

public class CustomerRegistered(Customer entity) : Event<Guid>(entity)
{
    public override string Key => "CUSTOMER_HAS_BEEN_REGISTERED";

    public string FullName { get; } = entity.FullName.Value;

    public DateOnly DateOfBirth { get; } = entity.DateOfBirth.Value;

    public string Email { get; } = entity.Email.Value;

    public string PhoneNumber { get; } = entity.PhoneNumber.Value;
}
