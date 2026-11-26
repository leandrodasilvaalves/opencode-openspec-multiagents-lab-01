using FluentAssertions;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Domain.Shared;
using Xunit;

namespace Lab.Orders.Domain.UnitTests.Customers;

public class CustomerTests
{
    private static readonly Document ValidDocument = Document.Create("52998224725");
    private static readonly Address ValidAddress = Address.Create("Rua A", "1", null, "Centro", "Cidade", "SP", "01001000");

    [Fact]
    public void Complete_registration_accepted()
    {
        var id = Guid.NewGuid();
        var customer = Customer.Create(id, "Maria Silva", "11999999999", ValidDocument, ValidAddress);

        customer.CustomerId.Should().Be(id);
        customer.Name.Should().Be("Maria Silva");
        customer.Phone.Should().Be("11999999999");
        customer.Document.Should().Be(ValidDocument);
        customer.Address.Should().Be(ValidAddress);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_name_rejected(string name)
    {
        var act = () => Customer.Create(Guid.NewGuid(), name, "11999999999", ValidDocument, ValidAddress);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Null_name_rejected()
    {
        var act = () => Customer.Create(Guid.NewGuid(), null, "11999999999", ValidDocument, ValidAddress);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_phone_rejected(string phone)
    {
        var act = () => Customer.Create(Guid.NewGuid(), "Maria", phone, ValidDocument, ValidAddress);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Null_phone_rejected()
    {
        var act = () => Customer.Create(Guid.NewGuid(), "Maria", null, ValidDocument, ValidAddress);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Missing_document_rejected()
    {
        var act = () => Customer.Create(Guid.NewGuid(), "Maria", "11999999999", default, ValidAddress);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Invalid_document_rejected()
    {
        var act = () => Document.Create("11111111111");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Missing_address_rejected()
    {
        var act = () => Customer.Create(Guid.NewGuid(), "Maria", "11999999999", ValidDocument, null);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Empty_customer_id_rejected()
    {
        var act = () => Customer.Create(Guid.Empty, "Maria", "11999999999", ValidDocument, ValidAddress);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Missing_address_subfield_rejected()
    {
        var act = () => Address.Create("Rua A", "1", null, "", "Cidade", "SP", "01001000");
        act.Should().Throw<DomainException>();
    }
}
