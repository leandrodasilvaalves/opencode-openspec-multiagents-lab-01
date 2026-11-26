using FluentAssertions;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Domain.Delivery;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Orders;
using Lab.Orders.Domain.Shared;
using Xunit;

namespace Lab.Orders.Domain.UnitTests.Discounts;

public class DiscountTests
{
    [Fact]
    public void Amount_discount_accepted()
    {
        var discount = Discount.ByAmount(new Money(5m));
        discount.Type.Should().Be(DiscountType.Amount);
        discount.Amount.Amount.Should().Be(5m);
        discount.CalculateDiscount(new Money(100m)).Amount.Should().Be(5m);
    }

    [Fact]
    public void Percentage_discount_accepted()
    {
        var discount = Discount.ByPercentage(new Percentage(10m));
        discount.Type.Should().Be(DiscountType.Percentage);
        discount.Percentage.Value.Should().Be(10m);
        discount.CalculateDiscount(new Money(100m)).Amount.Should().Be(10m);
    }

    [Fact]
    public void Single_modality_only_never_both()
    {
        var amount = Discount.ByAmount(new Money(5m));
        var percentage = Discount.ByPercentage(new Percentage(10m));

        amount.Type.Should().NotBe(percentage.Type);
        amount.Type.Should().Be(DiscountType.Amount);
        percentage.Type.Should().Be(DiscountType.Percentage);
    }

    [Fact]
    public void Negative_amount_discount_rejected()
    {
        var act = () => new Money(-1m);
        act.Should().Throw<DomainException>();
        var act2 = () => Discount.ByAmount(new Money(-1m));
        act2.Should().Throw<DomainException>();
    }

    [Fact]
    public void Negative_percentage_discount_rejected()
    {
        var act = () => new Percentage(-1m);
        act.Should().Throw<DomainException>();
        var act2 = () => Discount.ByPercentage(new Percentage(-1m));
        act2.Should().Throw<DomainException>();
    }

    [Fact]
    public void Null_discount_means_none()
    {
        Discount? none = null;
        none.Should().BeNull();
    }

    [Fact]
    public void Percentual_discount_rounded_once_to_cent_half_up()
    {
        Discount.ByPercentage(new Percentage(10m)).CalculateDiscount(new Money(0.05m)).Amount.Should().Be(0.01m);
        Discount.ByPercentage(new Percentage(33.33m)).CalculateDiscount(new Money(100m)).Amount.Should().Be(33.33m);
    }

    [Fact]
    public void Percentual_greater_than_hundred_rejected()
    {
        var act = () => Discount.ByPercentage(new Percentage(120m)).CalculateDiscount(new Money(100m));
        act.Should().Throw<DomainException>();
    }

    private static Customer Customer() => Lab.Orders.Domain.Customers.Customer.Create(
        Guid.NewGuid(), "Maria", "1199999", Document.Create("52998224725"),
        Address.Create("Rua A", "1", null, "Centro", "Cidade", "SP", "01001000"));

    private static OrderItem Item(string sku, Money basePrice, int qty, Discount? discount = null) =>
        OrderItem.Create(sku, qty, basePrice, discount, _ => true);

    private static Order MakeOrder(Discount? orderDiscount, params OrderItem[] items) =>
        Order.Create(Guid.NewGuid(), Customer(), items, PaymentMethod.Pix, orderDiscount, new Distance(10m));

    [Fact]
    public void No_item_discount_and_no_order_discount_allowed()
    {
        var order = MakeOrder(null, Item("A", new Money(100m), 1));
        order.Total.Amount.Should().Be(112m);
    }

    [Fact]
    public void Item_discount_alone_allowed()
    {
        var order = MakeOrder(null, Item("A", new Money(100m), 1, Discount.ByAmount(new Money(10m))));
        order.Total.Amount.Should().Be(102m);
    }

    [Fact]
    public void Order_discount_alone_allowed()
    {
        var order = MakeOrder(Discount.ByAmount(new Money(10m)), Item("A", new Money(100m), 1));
        order.Total.Amount.Should().Be(102m);
    }

    [Fact]
    public void Order_discount_with_item_discount_rejected()
    {
        var act = () => MakeOrder(
            Discount.ByAmount(new Money(10m)),
            Item("A", new Money(100m), 1, Discount.ByAmount(new Money(10m))));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Order_discount_smaller_than_subtotal_accepted()
    {
        var order = MakeOrder(Discount.ByAmount(new Money(20m)), Item("A", new Money(100m), 1));
        order.Total.Amount.Should().Be(92m);
    }

    [Fact]
    public void Order_discount_equal_to_subtotal_accepted()
    {
        var order = MakeOrder(Discount.ByAmount(new Money(100m)), Item("A", new Money(100m), 1));
        order.Total.Amount.Should().Be(12m);
    }

    [Fact]
    public void Order_discount_greater_than_subtotal_rejected()
    {
        var act = () => MakeOrder(Discount.ByAmount(new Money(120m)), Item("A", new Money(100m), 1));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Order_percentual_discount_greater_than_hundred_rejected()
    {
        var act = () => MakeOrder(Discount.ByPercentage(new Percentage(120m)), Item("A", new Money(100m), 1));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Floor_zero_before_delivery_fee()
    {
        var order = MakeOrder(Discount.ByAmount(new Money(100m)), Item("A", new Money(100m), 1));
        order.Total.Amount.Should().Be(order.DeliveryFee.Amount.Amount);
    }

    [Fact]
    public void Order_discount_does_not_affect_delivery_fee()
    {
        var noDiscount = MakeOrder(null, Item("A", new Money(100m), 1));
        var withDiscount = MakeOrder(Discount.ByAmount(new Money(20m)), Item("A", new Money(100m), 1));

        withDiscount.DeliveryFee.Amount.Amount.Should().Be(noDiscount.DeliveryFee.Amount.Amount);
        withDiscount.Total.Amount.Should().Be(92m);
    }

    [Fact]
    public void Percentual_order_discount_on_subtotal()
    {
        var order = MakeOrder(Discount.ByPercentage(new Percentage(10m)), Item("A", new Money(200m), 1));
        order.Total.Amount.Should().Be(192m);
    }
}
