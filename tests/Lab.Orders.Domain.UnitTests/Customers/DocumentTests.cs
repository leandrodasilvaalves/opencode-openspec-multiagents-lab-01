using FluentAssertions;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Domain.Shared;
using Xunit;

namespace Lab.Orders.Domain.UnitTests.Customers;

public class DocumentTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("529 982 247 25")]
    public void Valid_cpf_accepted_and_normalized(string input)
    {
        var doc = Document.Create(input);
        doc.Value.Should().Be("52998224725");
        doc.Kind.Should().Be(DocumentKind.Cpf);
    }

    [Theory]
    [InlineData("11.222.333/0001-81")]
    [InlineData("11222333000181")]
    public void Valid_cnpj_accepted_and_normalized(string input)
    {
        var doc = Document.Create(input);
        doc.Value.Should().Be("11222333000181");
        doc.Kind.Should().Be(DocumentKind.Cnpj);
    }

    [Theory]
    [InlineData("529.982.247-24")]
    [InlineData("52998224724")]
    [InlineData("111.444.777-36")]
    [InlineData("11111111111")]
    [InlineData("00000000000")]
    public void Invalid_cpf_check_digits_rejected(string input)
    {
        var act = () => Document.Create(input);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("11.222.333/0001-80")]
    [InlineData("11222333000180")]
    [InlineData("11111111111111")]
    public void Invalid_cnpj_check_digits_rejected(string input)
    {
        var act = () => Document.Create(input);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("123456789012345")]
    [InlineData("123")]
    [InlineData("")]
    [InlineData("   ")]
    public void Unknown_format_rejected(string input)
    {
        var act = () => Document.Create(input);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("5299822472A")]
    [InlineData("529.982.247-2X")]
    [InlineData("11.222.333/0001-8A")]
    [InlineData("ABCDEFGHIJK")]
    [InlineData("5299822472!")]
    public void Alphanumeric_rejected(string input)
    {
        var act = () => Document.Create(input);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Null_rejected()
    {
        var act = () => Document.Create(null);
        act.Should().Throw<DomainException>();
    }
}
