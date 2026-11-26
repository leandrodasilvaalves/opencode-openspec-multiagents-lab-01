using FluentAssertions;
using Lab.Orders.Domain.Shared;
using Xunit;

namespace Lab.Orders.Domain.UnitTests.Shared;

public class MoneyTests
{
    [Theory]
    [InlineData(1.005, 1.01)]
    [InlineData(1.015, 1.02)]
    [InlineData(1.004, 1.00)]
    [InlineData(1.001, 1.00)]
    [InlineData(2.675, 2.68)]
    [InlineData(0.125, 0.13)]
    [InlineData(0.005, 0.01)]
    [InlineData(10.999, 11.00)]
    public void Rounds_half_up_away_from_zero_to_two_decimals(decimal input, decimal expected)
    {
        new Money(input).Amount.Should().Be(expected);
    }

    [Fact]
    public void Keeps_two_decimal_values()
    {
        new Money(10.50m).Amount.Should().Be(10.50m);
        new Money(0m).Amount.Should().Be(0m);
    }

    [Fact]
    public void Rejects_negative_amounts()
    {
        var act = () => new Money(-0.01m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Zero_is_allowed()
    {
        Money.Zero.Amount.Should().Be(0m);
    }

    [Fact]
    public void Arithmetic_is_rounded_and_invariant()
    {
        (new Money(1.005m) + new Money(1.005m)).Amount.Should().Be(2.02m);
        (new Money(5m) - new Money(2.50m)).Amount.Should().Be(2.50m);
        (new Money(1.10m) * 3).Amount.Should().Be(3.30m);
    }

    [Fact]
    public void Subtraction_below_zero_is_rejected()
    {
        var act = () => new Money(1m) - new Money(2m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ToString_is_invariant_two_decimals()
    {
        new Money(1234.5m).ToString().Should().Be("1234.50");
    }
}
