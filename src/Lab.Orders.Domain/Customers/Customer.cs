using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Shared.ValueObjects;

namespace Lab.Orders.Domain.Customers;

public class Customer : Entity<Guid>
{
    public string Name { get; private set; }
    public string Phone { get; private set; }
    public Document Document { get; private set; }
    public Address Address { get; private set; }

    protected Customer() : base() { }

    public Customer(Guid id, string name, string phone, Document document, Address address)
    {
        Id = id;
        Name = name;
        Phone = phone;
        Document = document;
        Address = address;
        ValidateThrow();
    }

    protected override Guid GeneratedId() => Guid.NewGuid();

    protected override string ErrorCode() => "CUSTOMER";

    protected override void ValidateRules()
    {
        if (string.IsNullOrWhiteSpace(Name))
            AddError("Customer name is required.");
        if (string.IsNullOrWhiteSpace(Phone))
            AddError("Customer phone is required.");
        if (Document is null)
            AddError("Customer document is required.");
        if (Address is null)
            AddError("Customer address is required.");
    }
}
