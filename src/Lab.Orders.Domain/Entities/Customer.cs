using Lab.Orders.Domain.Commands.Customers;
using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Enums;
using Lab.Orders.Domain.Events.Customers;
using Lab.Orders.Domain.ValueObjects;

namespace Lab.Orders.Domain.Entities;

public class Customer : Entity
{
    public Name? Name { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public CustomerStatus Status { get; private set; }

    protected Customer() : base() { }

    protected override void ValidateRulers()
    {
        if (!IsOver18())
        {
            AddError("Customer must be 18 or older");
        }

        Validate(Name);
    }

    protected override string ErrorCode() => nameof(Customer).ToUpperInvariant();

    public void Register(IRegisterCustomer command)
    {
        Name = command.Name;
        DateOfBirth = command.DateOfBirth;
        UpdateAt = DateTimeOffset.UtcNow;
        Status = CustomerStatus.ACTIVE;
        Validate();
        AddEvent(new CustomerCreated(this));
    }

    public void Update(IUpdateCustomer command)
    {
        Name = command.Name;
        DateOfBirth = command.DateOfBirth;
        UpdateAt = DateTimeOffset.UtcNow;
        Validate();
        AddEvent(new CustomerUpdate(this));
    }

    public void Activate()
    {
        Status = CustomerStatus.ACTIVE;
        UpdateAt = DateTimeOffset.UtcNow;
        AddEvent(new CustomerActivated(this));
    }

    public void Inactivate()
    {
        Status = CustomerStatus.ACTIVE;
        UpdateAt = DateTimeOffset.UtcNow;
        AddEvent(new CustomerInactivated(this));
    }

    private bool IsOver18() => CaculateAge() >= 18;

    private int CaculateAge()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        int idade = today.Year - DateOfBirth.Year;

        if (DateOfBirth > today.AddYears(-idade))
        {
            idade--;
        }

        return idade;
    }

    public void Apply(CustomerCreated @event)
    {
        Name = @event.Name;
        DateOfBirth = @event.DateOfBirth;
        CreatedAt = @event.CreatedAt;
        UpdateAt  = @event.UpdatedAt;
    }

    public void Apply(CustomerUpdate @event)
    {
        Name = @event.Name;
        DateOfBirth = @event.DateOfBirth;
        UpdateAt  = @event.UpdatedAt;
    }

    public void Apply(CustomerActivated @event)
    {
        Status = @event.Status;
        UpdateAt = @event.UpdatedAt;
    }

    public void Apply(CustomerInactivated @event)
    {
        Status = @event.Status;
        UpdateAt = @event.UpdatedAt;
    }
}
