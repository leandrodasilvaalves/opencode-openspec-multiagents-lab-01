using FluentAssertions;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Products;
using Lab.Orders.Domain.Shared.ValueObjects;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class ProductTests
{
    [Fact]
    public void Complete_product_succeeds()
    {
        var product = new Product(Guid.NewGuid(), "SKU-001", "Widget", new Money(10m));
        product.Sku.Should().Be("SKU-001");
        product.Description.Should().Be("Widget");
        product.BasePrice.Amount.Should().Be(10m);
    }

    [Fact]
    public void Zero_base_price_is_accepted()
    {
        var product = new Product(Guid.NewGuid(), "SKU-001", "Widget", new Money(0m));
        product.BasePrice.Amount.Should().Be(0m);
    }

    [Fact]
    public void Empty_sku_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Product(Guid.NewGuid(), "", "Widget", new Money(10m)));
    }

    [Fact]
    public void Empty_description_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Product(Guid.NewGuid(), "SKU-001", "", new Money(10m)));
    }

    [Fact]
    public void Negative_base_price_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Product(Guid.NewGuid(), "SKU-001", "Widget", new Money(-1m)));
    }

    [Fact]
    public void Null_base_price_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Product(Guid.NewGuid(), "SKU-001", "Widget", null!));
    }
}
