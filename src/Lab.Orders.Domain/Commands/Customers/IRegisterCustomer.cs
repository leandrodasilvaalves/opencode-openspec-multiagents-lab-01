namespace Lab.Orders.Domain.Commands.Customers;

public interface IRegisterCustomer
{
    string Name { get; }
    string Document { get; }
    DateOnly DateOfBirth { get; }
}
