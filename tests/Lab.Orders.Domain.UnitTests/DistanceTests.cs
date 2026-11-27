using FluentAssertions;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Shared.ValueObjects;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class DistanceTests
{
    [Fact]
    public void Round_15_2_down_to_15()
    {
        var distance = new Distance(15.2m);
        distance.IntegerKilometers.Should().Be(15);
    }

    [Fact]
    public void Round_15_4_down_to_15()
    {
        var distance = new Distance(15.4m);
        distance.IntegerKilometers.Should().Be(15);
    }

    [Fact]
    public void Round_15_5_up_to_16()
    {
        var distance = new Distance(15.5m);
        distance.IntegerKilometers.Should().Be(16);
    }

    [Fact]
    public void Round_15_9_up_to_16()
    {
        var distance = new Distance(15.9m);
        distance.IntegerKilometers.Should().Be(16);
    }

    [Fact]
    public void Round_100_4_down_to_100()
    {
        var distance = new Distance(100.4m);
        distance.IntegerKilometers.Should().Be(100);
    }

    [Fact]
    public void Round_100_5_up_to_101()
    {
        var distance = new Distance(100.5m);
        distance.IntegerKilometers.Should().Be(101);
    }

    [Fact]
    public void Negative_distance_throws()
    {
        Assert.Throws<DomainRuleException>(() => new Distance(-1m));
    }

    [Fact]
    public void Zero_distance_succeeds()
    {
        var distance = new Distance(0m);
        distance.IntegerKilometers.Should().Be(0);
    }

    [Fact]
    public void Kilometers_property_stores_original()
    {
        var distance = new Distance(15.5m);
        distance.Kilometers.Should().Be(15.5m);
    }
}
