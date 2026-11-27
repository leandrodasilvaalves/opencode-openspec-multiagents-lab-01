using FluentAssertions;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Shared.ValueObjects;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class DocumentTests
{
    [Fact]
    public void Valid_cpf_is_accepted()
    {
        var doc = Document.Cpf("11144477735");
        doc.Number.Should().Be("11144477735");
        doc.Type.Should().Be("CPF");
    }

    [Fact]
    public void Valid_cnpj_is_accepted()
    {
        var doc = Document.Cnpj("11222333000181");
        doc.Number.Should().Be("11222333000181");
        doc.Type.Should().Be("CNPJ");
    }

    [Fact]
    public void Masked_cpf_is_accepted()
    {
        var doc = Document.FromString("111.444.777-35");
        doc.Number.Should().Be("11144477735");
    }

    [Fact]
    public void Masked_cnpj_is_accepted()
    {
        var doc = Document.FromString("11.222.333/0001-81");
        doc.Number.Should().Be("11222333000181");
    }

    [Fact]
    public void Invalid_cpf_dv_throws()
    {
        Assert.Throws<DomainRuleException>(() => Document.Cpf("11144477736"));
    }

    [Fact]
    public void Invalid_cnpj_dv_throws()
    {
        Assert.Throws<DomainRuleException>(() => Document.Cnpj("11222333000182"));
    }

    [Fact]
    public void Unknown_format_throws()
    {
        Assert.Throws<DomainRuleException>(() => Document.FromString("1234567890"));
    }

    [Fact]
    public void Empty_input_throws()
    {
        Assert.Throws<DomainRuleException>(() => Document.FromString(""));
    }

    [Fact]
    public void Whitespace_input_throws()
    {
        Assert.Throws<DomainRuleException>(() => Document.FromString("   "));
    }

    [Fact]
    public void Alphanumeric_input_throws()
    {
        Assert.Throws<DomainRuleException>(() => Document.FromString("abc123"));
    }

    [Fact]
    public void Cpf_with_wrong_length_throws()
    {
        Assert.Throws<DomainRuleException>(() => Document.Cpf("123456789"));
    }

    [Fact]
    public void Cnpj_with_wrong_length_throws()
    {
        Assert.Throws<DomainRuleException>(() => Document.Cnpj("1234567890123"));
    }
}
