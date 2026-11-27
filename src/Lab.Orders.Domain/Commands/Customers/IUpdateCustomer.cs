namespace Lab.Orders.Domain.Commands.Customers;

public interface IUpdateCustomer
{
    string Name { get; }
    DateOnly DateOfBirth { get; }
}