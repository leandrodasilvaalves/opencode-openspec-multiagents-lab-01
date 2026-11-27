using FluentAssertions;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Exceptions;
using Lab.Orders.Domain.Orders;
using Lab.Orders.Domain.Products;
using Lab.Orders.Domain.Shared.ValueObjects;
using Xunit;

namespace Lab.Orders.Domain.UnitTests;

public class OrderTests
{
    private readonly Guid _customerId = Guid.NewGuid();
    private readonly Product _product1 = new(Guid.NewGuid(), "SKU-001", "Widget", new Money(100m));
    private readonly Product _product2 = new(Guid.NewGuid(), "SKU-002", "Gadget", new Money(50m));

    private OrderItem CreateItem(Product product, int quantity, Discount? discount = null)
    {
        return new OrderItem(Guid.NewGuid(), product, quantity, discount);
    }

    [Fact]
    public void Complete_order_without_discount_succeeds()
    {
        var items = new[] { CreateItem(_product1, 1), CreateItem(_product2, 2) };
        var order = new Order(Guid.NewGuid(), _customerId, items, PaymentMethod.Pix, null, new Distance(10m));

        order.Subtotal.Amount.Should().Be(200m);
        order.OrderDiscountAmount.Amount.Should().Be(0m);
        order.Total.Amount.Should().Be(212m);
    }

    [Fact]
    public void Empty_customer_id_throws()
    {
        var items = new[] { CreateItem(_product1, 1) };
        Assert.Throws<DomainRuleException>(() =>
            new Order(Guid.NewGuid(), Guid.Empty, items, PaymentMethod.Cash, null, new Distance(10m)));
    }

    [Fact]
    public void No_items_throws()
    {
        Assert.Throws<DomainRuleException>(() =>
            new Order(Guid.NewGuid(), _customerId, Enumerable.Empty<OrderItem>(), PaymentMethod.Cash, null, new Distance(10m)));
    }

    [Fact]
    public void Invalid_payment_method_throws()
    {
        var items = new[] { CreateItem(_product1, 1) };
        Assert.Throws<DomainRuleException>(() =>
            new Order(Guid.NewGuid(), _customerId, items, (PaymentMethod)99, null, new Distance(10m)));
    }

    [Fact]
    public void Amount_discount_less_than_subtotal()
    {
        var items = new[] { CreateItem(_product1, 1) };
        var order = new Order(Guid.NewGuid(), _customerId, items, PaymentMethod.Cash,
            Discount.OfAmount(new Money(30m)), new Distance(10m));

        order.Subtotal.Amount.Should().Be(100m);
        order.OrderDiscountAmount.Amount.Should().Be(30m);
        order.Total.Amount.Should().Be(82m);
    }

    [Fact]
    public void Percentage_discount_on_subtotal()
    {
        var items = new[] { CreateItem(_product1, 1), CreateItem(_product2, 1) };
        var order = new Order(Guid.NewGuid(), _customerId, items, PaymentMethod.Cash,
            Discount.OfPercentage(new Percentage(10m)), new Distance(10m));

        order.Subtotal.Amount.Should().Be(150m);
        order.OrderDiscountAmount.Amount.Should().Be(15m);
        order.Total.Amount.Should().Be(147m);
    }

    [Fact]
    public void Discount_equals_subtotal_results_zero_parcel()
    {
        var items = new[] { CreateItem(_product1, 1) };
        var order = new Order(Guid.NewGuid(), _customerId, items, PaymentMethod.Cash,
            Discount.OfAmount(new Money(100m)), new Distance(10m));

        order.Subtotal.Amount.Should().Be(100m);
        order.OrderDiscountAmount.Amount.Should().Be(100m);
        order.Total.Amount.Should().Be(12m);
    }

    [Fact]
    public void Discount_greater_than_subtotal_throws()
    {
        var items = new[] { CreateItem(_product1, 1) };
        Assert.Throws<DomainRuleException>(() =>
            new Order(Guid.NewGuid(), _customerId, items, PaymentMethod.Cash,
                Discount.OfAmount(new Money(101m)), new Distance(10m)));
    }

    [Fact]
    public void Distance_over_100_km_throws()
    {
        var items = new[] { CreateItem(_product1, 1) };
        Assert.Throws<DomainRuleException>(() =>
            new Order(Guid.NewGuid(), _customerId, items, PaymentMethod.Cash, null, new Distance(101m)));
    }

    [Fact]
    public void Order_level_discount_with_item_level_discount_throws()
    {
        var itemDiscount = Discount.OfAmount(new Money(10m));
        var item = new OrderItem(Guid.NewGuid(), _product1, 1, itemDiscount);
        var items = new[] { item };

        Assert.Throws<DomainRuleException>(() =>
            new Order(Guid.NewGuid(), _customerId, items, PaymentMethod.Cash,
                Discount.OfPercentage(new Percentage(10m)), new Distance(10m)));
    }

    [Fact]
    public void All_payment_methods_accepted()
    {
        var items = new[] { CreateItem(_product1, 1) };
        foreach (var method in Enum.GetValues<PaymentMethod>())
        {
            var order = new Order(Guid.NewGuid(), _customerId, items, method, null, new Distance(10m));
            order.PaymentMethod.Should().Be(method);
        }
    }

    [Fact]
    public void Delivery_fee_is_not_discounted()
    {
        var items = new[] { CreateItem(_product1, 1) };
        var order = new Order(Guid.NewGuid(), _customerId, items, PaymentMethod.Cash,
            Discount.OfAmount(new Money(100m)), new Distance(20m));

        order.Total.Amount.Should().Be(17m);
    }
}
