using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Domain.Customers;

public sealed class Customer
{
    public Guid CustomerId { get; }
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public Document Document { get; private set; }
    public Address Address { get; private set; } = null!;

    private Customer()
    {
    }

    private Customer(Guid customerId, string name, string phone, Document document, Address address)
    {
        CustomerId = customerId;
        Name = name;
        Phone = phone;
        Document = document;
        Address = address;
    }

    public static Customer Create(Guid customerId, string? name, string? phone, Document document, Address? address)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("CustomerId is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer name is required.");

        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("Customer phone is required.");

        if (document == default)
            throw new DomainException("Customer document is required.");

        if (address is null)
            throw new DomainException("Customer address is required.");

        return new Customer(customerId, name.Trim(), phone.Trim(), document, address);
    }
}
