using FluentAssertions;
using Lab.Orders.Domain.Products;
using Lab.Orders.Domain.Shared;
using Xunit;

namespace Lab.Orders.Domain.UnitTests.Products;

public class ProductTests
{
    [Fact]
    public void Complete_product_accepted()
    {
        var id = Guid.NewGuid();
        var product = Product.Create(id, "  SKU-1 ", "Coffee", new Money(10.50m));

        product.ProductId.Should().Be(id);
        product.Sku.Should().Be("SKU-1");
        product.Description.Should().Be("Coffee");
        product.BasePrice.Amount.Should().Be(10.50m);
    }

    [Fact]
    public void Zero_base_price_accepted()
    {
        Product.Create(Guid.NewGuid(), "SKU", "Free", new Money(0m)).BasePrice.Amount.Should().Be(0m);
    }

    [Fact]
    public void Negative_base_price_rejected()
    {
        var act = () => new Money(-1m);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_sku_rejected(string? sku)
    {
        var act = () => Product.Create(Guid.NewGuid(), sku, "Coffee", new Money(1m));
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_description_rejected(string? description)
    {
        var act = () => Product.Create(Guid.NewGuid(), "SKU", description, new Money(1m));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Empty_product_id_rejected()
    {
        var act = () => Product.Create(Guid.Empty, "SKU", "Coffee", new Money(1m));
        act.Should().Throw<DomainException>();
    }
}
