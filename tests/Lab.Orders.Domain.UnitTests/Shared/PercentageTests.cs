using FluentAssertions;
using Lab.Orders.Domain.Shared;
using Xunit;

namespace Lab.Orders.Domain.UnitTests.Shared;

public class PercentageTests
{
    [Fact]
    public void Accepts_zero_and_positive()
    {
        new Percentage(0m).Value.Should().Be(0m);
        new Percentage(10m).Value.Should().Be(10m);
        new Percentage(100m).Value.Should().Be(100m);
    }

    [Fact]
    public void Rejects_negative()
    {
        var act = () => new Percentage(-1m);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Applies_percentage_and_rounds_once_to_cent()
    {
        new Percentage(10m).ApplyTo(new Money(100m)).Amount.Should().Be(10m);
        new Percentage(12.5m).ApplyTo(new Money(100m)).Amount.Should().Be(12.50m);
        new Percentage(10m).ApplyTo(new Money(0.05m)).Amount.Should().Be(0.01m);
        new Percentage(33.33m).ApplyTo(new Money(100m)).Amount.Should().Be(33.33m);
    }

    [Fact]
    public void Hundred_percent_applies_fully()
    {
        new Percentage(100m).ApplyTo(new Money(50m)).Amount.Should().Be(50m);
    }

    [Fact]
    public void Greater_than_hundred_rejected_when_applied()
    {
        var act = () => new Percentage(100.01m).ApplyTo(new Money(50m));
        act.Should().Throw<DomainException>();
        new Percentage(100.01m).Value.Should().Be(100.01m);
    }

    [Fact]
    public void Rejects_double_usage_indirectly_by_decimal_only_api()
    {
        new Percentage(0.1m).ApplyTo(new Money(1000m)).Amount.Should().Be(1m);
    }
}
