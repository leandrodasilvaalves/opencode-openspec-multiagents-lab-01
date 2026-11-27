using FluentAssertions;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Shared.ValueObjects;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class CustomerTests
{
    private readonly Document _document = Document.Cpf("11144477735");
    private readonly Address _address = new("Rua das Flores", "123", null, "Centro", "Sao Paulo", "SP", "01234567");

    [Fact]
    public void Complete_customer_succeeds()
    {
        var customer = new Customer(Guid.NewGuid(), "John Doe", "11999999999", _document, _address);
        customer.Name.Should().Be("John Doe");
        customer.Phone.Should().Be("11999999999");
        customer.Document.Should().Be(_document);
        customer.Address.Should().Be(_address);
    }

    [Fact]
    public void Empty_name_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Customer(Guid.NewGuid(), "", "11999999999", _document, _address));
    }

    [Fact]
    public void Empty_phone_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Customer(Guid.NewGuid(), "John Doe", "", _document, _address));
    }

    [Fact]
    public void Null_document_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Customer(Guid.NewGuid(), "John Doe", "11999999999", null!, _address));
    }

    [Fact]
    public void Null_address_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Customer(Guid.NewGuid(), "John Doe", "11999999999", _document, null!));
    }

    [Fact]
    public void Whitespace_name_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Customer(Guid.NewGuid(), "   ", "11999999999", _document, _address));
    }

    [Fact]
    public void Whitespace_phone_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Customer(Guid.NewGuid(), "John Doe", "   ", _document, _address));
    }
}
