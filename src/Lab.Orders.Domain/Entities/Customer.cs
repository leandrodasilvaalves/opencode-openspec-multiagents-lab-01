using Lab.Orders.Domain.Commands.Customers;
using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Events.Customers;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.ValueObjects;

namespace Lab.Orders.Domain.Entities;

public class Customer : Entity
{
    public FullName FullName { get; private set; } = null!;

    public DateOfBirth DateOfBirth { get; private set; } = null!;

    public Email Email { get; private set; } = null!;

    public PhoneNumber PhoneNumber { get; private set; } = null!;

    protected Customer() : base() { }

    protected override string ErrorCode() => "CUSTOMER";

    public void Register(IRegisterCustomer command)
    {
        DomainNullException.ThrowIfNull(command);

        FullName = new FullName(command.FullName);
        DateOfBirth = new DateOfBirth(command.DateOfBirth);
        Email = new Email(command.Email);
        PhoneNumber = new PhoneNumber(command.PhoneNumber);
        UpdatedAt = DateTimeOffset.UtcNow;

        Validate();
        AddEvent(new CustomerRegistered(this));
    }

    public void Apply(CustomerRegistered @event)
    {
        FullName = new FullName(@event.FullName);
        DateOfBirth = new DateOfBirth(@event.DateOfBirth);
        Email = new Email(@event.Email);
        PhoneNumber = new PhoneNumber(@event.PhoneNumber);
        UpdatedAt = @event.UpdatedAt;
    }

    protected override void ValidateRules()
    {
        if (FullName is null)
        {
            AddError("Full name is required.");
        }

        if (DateOfBirth is null)
        {
            AddError("Date of birth is required.");
        }

        if (Email is null)
        {
            AddError("Email is required.");
        }

        if (PhoneNumber is null)
        {
            AddError("Phone number is required.");
        }

        if (FullName is not null)
        {
            Validate(FullName);
        }

        if (DateOfBirth is not null)
        {
            Validate(DateOfBirth);
        }

        if (Email is not null)
        {
            Validate(Email);
        }

        if (PhoneNumber is not null)
        {
            Validate(PhoneNumber);
        }
    }
}
