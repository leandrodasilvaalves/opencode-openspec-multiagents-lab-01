namespace Lab.Orders.Domain.Commands.Customers;

public interface IRegisterCustomer
{
    string FullName { get; }
    DateOnly DateOfBirth { get; }
    string Email { get; }
    string PhoneNumber { get; }
}
