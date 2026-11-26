using Lab.Orders.Application.Common;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Application.Customers;

public sealed record RegisterCustomerCommand(
    string? Name,
    string? Phone,
    string? Document,
    string? Street,
    string? Number,
    string? Complement,
    string? Neighborhood,
    string? City,
    string? State,
    string? PostalCode);

public sealed record RegisterCustomerResult(
    Guid CustomerId,
    string Name,
    string Phone,
    string Document,
    DocumentKind DocumentKind,
    Address Address);

public sealed class RegisterCustomer
{
    private readonly ICustomerRepository _customerRepository;

    public RegisterCustomer(ICustomerRepository customerRepository) => _customerRepository = customerRepository;

    public async Task<Result<RegisterCustomerResult>> ExecuteAsync(
        RegisterCustomerCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Customer customer;

        try
        {
            var document = Document.Create(command.Document);
            var address = Address.Create(
                command.Street,
                command.Number,
                command.Complement,
                command.Neighborhood,
                command.City,
                command.State,
                command.PostalCode);

            customer = Customer.Create(Guid.NewGuid(), command.Name, command.Phone, document, address);
        }
        catch (DomainException exception)
        {
            return Result<RegisterCustomerResult>.Validation("InvalidCustomer", exception.Message);
        }

        await _customerRepository.AddAsync(customer, cancellationToken);

        return Result<RegisterCustomerResult>.Ok(new RegisterCustomerResult(
            customer.CustomerId,
            customer.Name,
            customer.Phone,
            customer.Document.Value,
            customer.Document.Kind,
            customer.Address));
    }
}
