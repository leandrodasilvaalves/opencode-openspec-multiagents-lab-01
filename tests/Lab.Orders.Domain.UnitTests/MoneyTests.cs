using FluentAssertions;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Shared.ValueObjects;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class MoneyTests
{
    [Fact]
    public void Create_positive_amount_succeeds()
    {
        var money = new Money(10.50m);
        money.Amount.Should().Be(10.50m);
    }

    [Fact]
    public void Create_zero_amount_succeeds()
    {
        var money = new Money(0m);
        money.Amount.Should().Be(0m);
    }

    [Fact]
    public void Create_negative_amount_throws()
    {
        Assert.Throws<DomainRuleException>(() => new Money(-1m));
    }

    [Fact]
    public void Round_half_up_to_two_decimals()
    {
        var money = new Money(10.005m);
        money.Amount.Should().Be(10.01m);

        var money2 = new Money(10.004m);
        money2.Amount.Should().Be(10m);
    }

    [Fact]
    public void Add_returns_sum()
    {
        var result = new Money(10m) + new Money(5.25m);
        result.Amount.Should().Be(15.25m);
    }

    [Fact]
    public void Subtract_returns_difference()
    {
        var result = new Money(10m) - new Money(3.50m);
        result.Amount.Should().Be(6.50m);
    }

    [Fact]
    public void Multiply_returns_product()
    {
        var result = new Money(10m).Multiply(3);
        result.Amount.Should().Be(30m);
    }

    [Fact]
    public void Comparison_operators_work()
    {
        (new Money(10m) > new Money(5m)).Should().BeTrue();
        (new Money(5m) < new Money(10m)).Should().BeTrue();
        (new Money(10m) >= new Money(10m)).Should().BeTrue();
        (new Money(5m) <= new Money(10m)).Should().BeTrue();
    }

    [Fact]
    public void ToString_uses_invariant_culture()
    {
        var money = new Money(10.5m);
        money.ToString().Should().Be("10.50");
    }

    [Fact]
    public void Equal_amounts_are_equal()
    {
        var a = new Money(10m);
        var b = new Money(10m);
        a.Equals(b).Should().BeTrue();
    }
}
