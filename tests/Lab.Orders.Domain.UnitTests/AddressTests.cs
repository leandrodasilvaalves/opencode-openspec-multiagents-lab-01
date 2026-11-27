using FluentAssertions;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Domain.Exceptions;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class AddressTests
{
    [Fact]
    public void Complete_address_succeeds()
    {
        var address = new Address("Rua das Flores", "123", "Apto 4", "Centro", "Sao Paulo", "SP", "01234-567");
        address.Street.Should().Be("Rua das Flores");
        address.Number.Should().Be("123");
        address.Complement.Should().Be("Apto 4");
        address.Neighborhood.Should().Be("Centro");
        address.City.Should().Be("Sao Paulo");
        address.State.Should().Be("SP");
        address.PostalCode.Should().Be("01234567");
    }

    [Fact]
    public void Address_without_complement_succeeds()
    {
        var address = new Address("Rua das Flores", "123", null, "Centro", "Sao Paulo", "SP", "01234567");
        address.Complement.Should().BeNull();
    }

    [Fact]
    public void Empty_street_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Address("", "123", null, "Centro", "Sao Paulo", "SP", "01234567"));
    }

    [Fact]
    public void Empty_number_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Address("Rua das Flores", "", null, "Centro", "Sao Paulo", "SP", "01234567"));
    }

    [Fact]
    public void Empty_neighborhood_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Address("Rua das Flores", "123", null, "", "Sao Paulo", "SP", "01234567"));
    }

    [Fact]
    public void Empty_city_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Address("Rua das Flores", "123", null, "Centro", "", "SP", "01234567"));
    }

    [Fact]
    public void Invalid_state_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Address("Rua das Flores", "123", null, "Centro", "Sao Paulo", "SAO", "01234567"));
    }

    [Fact]
    public void Empty_state_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Address("Rua das Flores", "123", null, "Centro", "Sao Paulo", "", "01234567"));
    }

    [Fact]
    public void Empty_postal_code_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Address("Rua das Flores", "123", null, "Centro", "Sao Paulo", "SP", ""));
    }

    [Fact]
    public void Invalid_postal_code_length_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Address("Rua das Flores", "123", null, "Centro", "Sao Paulo", "SP", "12345"));
    }

    [Fact]
    public void Postal_code_normalizes_hyphen()
    {
        var address = new Address("Rua das Flores", "123", null, "Centro", "Sao Paulo", "SP", "01234-567");
        address.PostalCode.Should().Be("01234567");
    }

    [Fact]
    public void State_is_normalized_to_uppercase()
    {
        var address = new Address("Rua das Flores", "123", null, "Centro", "Sao Paulo", "sp", "01234567");
        address.State.Should().Be("SP");
    }
}
