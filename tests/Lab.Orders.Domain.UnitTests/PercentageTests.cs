using FluentAssertions;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Shared.ValueObjects;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class PercentageTests
{
    [Fact]
    public void Create_valid_percentage_succeeds()
    {
        var pct = new Percentage(50m);
        pct.Value.Should().Be(50m);
    }

    [Fact]
    public void Create_zero_percentage_succeeds()
    {
        var pct = new Percentage(0m);
        pct.Value.Should().Be(0m);
    }

    [Fact]
    public void Create_negative_percentage_throws()
    {
        Assert.Throws<DomainRuleException>(() => new Percentage(-1m));
    }

    [Fact]
    public void ApplyTo_returns_calculated_value()
    {
        var pct = new Percentage(50m);
        pct.ApplyTo(100m).Should().Be(50m);
    }

    [Fact]
    public void ApplyTo_ten_percent_of_two_hundred()
    {
        var pct = new Percentage(10m);
        pct.ApplyTo(200m).Should().Be(20m);
    }

    [Fact]
    public void ApplyTo_over_100_throws()
    {
        var pct = new Percentage(150m);
        Assert.Throws<DomainRuleException>(() => pct.ApplyTo(100m));
    }

    [Fact]
    public void ApplyTo_rounds_half_up_once()
    {
        var pct = new Percentage(33.33m);
        pct.ApplyTo(100m).Should().Be(33.33m);
    }

    [Fact]
    public void ToString_uses_invariant_culture()
    {
        var pct = new Percentage(12.5m);
        pct.ToString().Should().Be("12.50");
    }
}
