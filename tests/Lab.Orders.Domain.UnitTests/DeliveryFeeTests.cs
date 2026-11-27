using FluentAssertions;
using Lab.Orders.Domain.Delivery;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Shared.ValueObjects;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class DeliveryFeeTests
{
    [Theory]
    [InlineData(10, 12.00)]
    [InlineData(15, 12.00)]
    [InlineData(16, 13.00)]
    [InlineData(20, 17.00)]
    [InlineData(100, 97.00)]
    public void Calculate_fee_for_valid_distance(int km, decimal expected)
    {
        var distance = new Distance(km);
        var fee = new DeliveryFee(distance);
        fee.Amount.Amount.Should().Be(expected);
    }

    [Fact]
    public void Distance_over_100_km_throws()
    {
        var distance = new Distance(101m);
        Assert.Throws<DomainRuleException>(() => new DeliveryFee(distance));
    }

    [Fact]
    public void Distance_100_km_is_accepted()
    {
        var distance = new Distance(100m);
        var fee = new DeliveryFee(distance);
        fee.Amount.Amount.Should().Be(97m);
    }

    [Fact]
    public void Rounding_distance_before_fee()
    {
        var distance = new Distance(15.5m);
        var fee = new DeliveryFee(distance);
        fee.Amount.Amount.Should().Be(13m);
    }

    [Fact]
    public void Rounding_distance_100_5_throws()
    {
        var distance = new Distance(100.5m);
        Assert.Throws<DomainRuleException>(() => new DeliveryFee(distance));
    }
}
