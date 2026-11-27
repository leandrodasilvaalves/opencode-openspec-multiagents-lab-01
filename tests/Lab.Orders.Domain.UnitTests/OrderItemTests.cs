using FluentAssertions;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Orders;
using Lab.Orders.Domain.Products;
using Lab.Orders.Domain.Shared.ValueObjects;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class OrderItemTests
{
    private readonly Product _product = new(Guid.NewGuid(), "SKU-001", "Widget", new Money(100m));

    [Fact]
    public void Complete_order_item_succeeds()
    {
        var item = new OrderItem(Guid.NewGuid(), _product, 2, null);
        item.Sku.Should().Be("SKU-001");
        item.Quantity.Should().Be(2);
        item.FinalPrice.Amount.Should().Be(200m);
    }

    [Fact]
    public void Zero_quantity_throws()
    {
        Assert.Throws<DomainRuleException>(() => new OrderItem(Guid.NewGuid(), _product, 0, null));
    }

    [Fact]
    public void Negative_quantity_throws()
    {
        Assert.Throws<DomainRuleException>(() => new OrderItem(Guid.NewGuid(), _product, -1, null));
    }

    [Fact]
    public void Discount_equals_base_price_returns_zero()
    {
        var discount = Discount.OfAmount(new Money(100m));
        var item = new OrderItem(Guid.NewGuid(), _product, 1, discount);
        item.FinalPrice.Amount.Should().Be(0m);
    }

    [Fact]
    public void Discount_greater_than_base_price_throws()
    {
        var discount = Discount.OfAmount(new Money(101m));
        Assert.Throws<DomainRuleException>(() => new OrderItem(Guid.NewGuid(), _product, 1, discount));
    }

    [Fact]
    public void Percentage_discount_over_100_throws()
    {
        var discount = Discount.OfPercentage(new Percentage(150m));
        Assert.Throws<DomainRuleException>(() => new OrderItem(Guid.NewGuid(), _product, 1, discount));
    }

    [Fact]
    public void Percentage_discount_calculates_final_price()
    {
        var discount = Discount.OfPercentage(new Percentage(10m));
        var item = new OrderItem(Guid.NewGuid(), _product, 2, discount);
        item.FinalPrice.Amount.Should().Be(180m);
    }

    [Fact]
    public void Amount_discount_calculates_final_price()
    {
        var discount = Discount.OfAmount(new Money(10m));
        var item = new OrderItem(Guid.NewGuid(), _product, 3, discount);
        item.FinalPrice.Amount.Should().Be(270m);
    }

    [Fact]
    public void Final_price_rounds_half_up()
    {
        var product = new Product(Guid.NewGuid(), "SKU-002", "Gadget", new Money(10.005m));
        var item = new OrderItem(Guid.NewGuid(), product, 1, null);
        item.FinalPrice.Amount.Should().Be(10.01m);
    }
}
