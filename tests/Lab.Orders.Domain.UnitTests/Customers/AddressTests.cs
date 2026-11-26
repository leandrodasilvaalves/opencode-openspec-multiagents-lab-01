using FluentAssertions;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Domain.Shared;
using Xunit;

namespace Lab.Orders.Domain.UnitTests.Customers;

public class AddressTests
{
    public static Address Valid() => Address.Create(
        "Rua das Flores", "123", null, "Centro", "Sao Paulo", "SP", "01001-000");

    [Fact]
    public void Valid_address_accepted_and_normalized()
    {
        var address = Address.Create(" Rua A ", " 10 ", " Apto 2 ", " Centro ", " Sao Paulo ", "sp", "01001-000");

        address.Street.Should().Be("Rua A");
        address.Number.Should().Be("10");
        address.Complement.Should().Be("Apto 2");
        address.Neighborhood.Should().Be("Centro");
        address.City.Should().Be("Sao Paulo");
        address.State.Should().Be("SP");
        address.PostalCode.Should().Be("01001000");
    }

    [Fact]
    public void Complement_is_optional()
    {
        Valid().Complement.Should().BeNull();
        Address.Create("Rua A", "1", "", "Centro", "Cidade", "SP", "01001000").Complement.Should().BeNull();
    }

    [Fact]
    public void Postal_code_normalized()
    {
        Address.Create("Rua A", "1", null, "Centro", "Cidade", "SP", "01001-000").PostalCode.Should().Be("01001000");
    }

    [Theory]
    [InlineData(null, "1", null, "Centro", "Cidade", "SP", "01001000")]
    [InlineData("", "1", null, "Centro", "Cidade", "SP", "01001000")]
    [InlineData("Rua A", null, null, "Centro", "Cidade", "SP", "01001000")]
    [InlineData("Rua A", "", null, "Centro", "Cidade", "SP", "01001000")]
    [InlineData("Rua A", "1", null, null, "Cidade", "SP", "01001000")]
    [InlineData("Rua A", "1", null, "", "Cidade", "SP", "01001000")]
    [InlineData("Rua A", "1", null, "Centro", null, "SP", "01001000")]
    [InlineData("Rua A", "1", null, "Centro", "", "SP", "01001000")]
    [InlineData("Rua A", "1", null, "Centro", "Cidade", null, "01001000")]
    [InlineData("Rua A", "1", null, "Centro", "Cidade", "", "01001000")]
    [InlineData("Rua A", "1", null, "Centro", "Cidade", "SP", null)]
    [InlineData("Rua A", "1", null, "Centro", "Cidade", "SP", "")]
    public void Required_fields_rejected(string? street, string? number, string? complement, string? neighborhood, string? city, string? state, string? postalCode)
    {
        var act = () => Address.Create(street, number, complement, neighborhood, city, state, postalCode);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("S")]
    [InlineData("SPA")]
    [InlineData("XX")]
    [InlineData("12")]
    public void Invalid_state_rejected(string state)
    {
        var act = () => Address.Create("Rua A", "1", null, "Centro", "Cidade", state, "01001000");
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("0100100")]
    [InlineData("010010000")]
    [InlineData("01001-00A")]
    [InlineData("01001A000")]
    public void Invalid_postal_code_rejected(string postalCode)
    {
        var act = () => Address.Create("Rua A", "1", null, "Centro", "Cidade", "SP", postalCode);
        act.Should().Throw<DomainException>();
    }
}
