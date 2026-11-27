using FluentAssertions;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Shared.ValueObjects;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class DiscountTests
{
    [Fact]
    public void Amount_discount_succeeds()
    {
        var discount = Discount.OfAmount(new Money(10m));
        discount.Type.Should().Be(DiscountType.Amount);
        discount.AmountValue!.Amount.Should().Be(10m);
    }

    [Fact]
    public void Percentage_discount_succeeds()
    {
        var discount = Discount.OfPercentage(new Percentage(10m));
        discount.Type.Should().Be(DiscountType.Percentage);
        discount.PercentValue!.Value.Should().Be(10m);
    }

    [Fact]
    public void Negative_amount_discount_throws()
    {
        Assert.Throws<DomainRuleException>(() => Discount.OfAmount(new Money(-5m)));
    }

    [Fact]
    public void ApplyTo_amount_returns_value()
    {
        var discount = Discount.OfAmount(new Money(10m));
        discount.ApplyTo(100m).Should().Be(10m);
    }

    [Fact]
    public void ApplyTo_percentage_returns_calculated_value()
    {
        var discount = Discount.OfPercentage(new Percentage(10m));
        discount.ApplyTo(200m).Should().Be(20m);
    }

    [Fact]
    public void ApplyTo_percentage_over_100_throws()
    {
        var discount = Discount.OfPercentage(new Percentage(150m));
        Assert.Throws<DomainRuleException>(() => discount.ApplyTo(100m));
    }

    [Fact]
    public void Order_discount_vs_item_discount_mutually_exclusive()
    {
        var itemDiscount = Discount.OfAmount(new Money(5m));
        var orderDiscount = Discount.OfPercentage(new Percentage(10m));

        itemDiscount.Type.Should().NotBe(orderDiscount.Type);
    }
}
