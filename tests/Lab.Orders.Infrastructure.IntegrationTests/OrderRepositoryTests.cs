using FluentAssertions;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Domain.Delivery;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Orders;
using Lab.Orders.Domain.Shared;
using Lab.Orders.Infrastructure.Repositories;
using Xunit;

namespace Lab.Orders.Infrastructure.IntegrationTests;

public class OrderRepositoryTests
{
    private const string ValidCpf = "52998224725";

    private static Customer CreateCustomer()
    {
        var document = Document.Create(ValidCpf);
        var address = Address.Create(
            "Rua Oscar Freire",
            "500",
            null,
            "Cerqueira César",
            "São Paulo",
            "SP",
            "01426000");

        return Customer.Create(Guid.NewGuid(), "Alice Smith", "11977776666", document, address);
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_WithOrderDiscount_ShouldPersistAndRetrieveFullOrder()
    {
        var dbName = Guid.NewGuid().ToString();
        var customer = CreateCustomer();

        await using (var context = DbContextFactory.Create(dbName))
        {
            var customerRepo = new CustomerRepository(context);
            await customerRepo.AddAsync(customer);
        }

        var item1 = OrderItem.Create("PROD-01", 2, new Money(50.00m), null, _ => true);
        var item2 = OrderItem.Create("PROD-02", 1, new Money(100.00m), null, _ => true);
        var orderDiscount = Discount.ByAmount(new Money(20.00m));
        var distance = new Distance(20.0m);
        var orderId = Guid.NewGuid();

        var order = Order.Create(
            orderId,
            customer,
            [item1, item2],
            PaymentMethod.CreditCard,
            orderDiscount,
            distance);

        await using (var context = DbContextFactory.Create(dbName))
        {
            var orderRepo = new OrderRepository(context);
            await orderRepo.AddAsync(order);
        }

        await using (var context = DbContextFactory.Create(dbName))
        {
            var orderRepo = new OrderRepository(context);
            var retrieved = await orderRepo.GetByIdAsync(orderId);

            retrieved.Should().NotBeNull();
            retrieved!.OrderId.Should().Be(orderId);
            retrieved.CustomerId.Should().Be(customer.CustomerId);
            retrieved.Customer.Should().NotBeNull();
            retrieved.Customer.Name.Should().Be("Alice Smith");
            retrieved.PaymentMethod.Should().Be(PaymentMethod.CreditCard);

            retrieved.Discount.Should().NotBeNull();
            retrieved.Discount!.Type.Should().Be(DiscountType.Amount);
            retrieved.Discount.Amount.Amount.Should().Be(20.00m);

            retrieved.DistanceKm.Km.Should().Be(20.0m);
            retrieved.DistanceKm.RoundedKm.Should().Be(20);

            retrieved.Subtotal.Amount.Should().Be(200.00m);
            retrieved.OrderDiscount.Amount.Should().Be(20.00m);
            retrieved.DeliveryFee.Amount.Amount.Should().Be(17.00m);
            retrieved.Total.Amount.Should().Be(197.00m);

            retrieved.OrderItems.Should().HaveCount(2);
            retrieved.OrderItems.Should().Contain(i => i.Sku == "PROD-01" && i.Quantity == 2 && i.BasePrice.Amount == 50.00m && i.LineTotal.Amount == 100.00m);
            retrieved.OrderItems.Should().Contain(i => i.Sku == "PROD-02" && i.Quantity == 1 && i.BasePrice.Amount == 100.00m && i.LineTotal.Amount == 100.00m);
        }
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_WithItemDiscount_ShouldPersistAndRetrieveFullOrder()
    {
        var dbName = Guid.NewGuid().ToString();
        var customer = CreateCustomer();

        await using (var context = DbContextFactory.Create(dbName))
        {
            var customerRepo = new CustomerRepository(context);
            await customerRepo.AddAsync(customer);
        }

        var itemDiscount = Discount.ByPercentage(new Percentage(10.0m));
        var item = OrderItem.Create("PROD-10", 3, new Money(100.00m), itemDiscount, _ => true);
        var distance = new Distance(10.0m);
        var orderId = Guid.NewGuid();

        var order = Order.Create(
            orderId,
            customer,
            [item],
            PaymentMethod.Pix,
            null,
            distance);

        await using (var context = DbContextFactory.Create(dbName))
        {
            var orderRepo = new OrderRepository(context);
            await orderRepo.AddAsync(order);
        }

        await using (var context = DbContextFactory.Create(dbName))
        {
            var orderRepo = new OrderRepository(context);
            var retrieved = await orderRepo.GetByIdAsync(orderId);

            retrieved.Should().NotBeNull();
            retrieved!.OrderId.Should().Be(orderId);
            retrieved.Discount.Should().BeNull();
            retrieved.PaymentMethod.Should().Be(PaymentMethod.Pix);

            retrieved.Subtotal.Amount.Should().Be(270.00m);
            retrieved.OrderDiscount.Amount.Should().Be(0.00m);
            retrieved.DeliveryFee.Amount.Amount.Should().Be(12.00m);
            retrieved.Total.Amount.Should().Be(282.00m);

            retrieved.OrderItems.Should().HaveCount(1);
            var retrievedItem = retrieved.OrderItems.Single();
            retrievedItem.Sku.Should().Be("PROD-10");
            retrievedItem.Quantity.Should().Be(3);
            retrievedItem.BasePrice.Amount.Should().Be(100.00m);
            retrievedItem.LineTotal.Amount.Should().Be(270.00m);
            retrievedItem.Discount.Should().NotBeNull();
            retrievedItem.Discount!.Type.Should().Be(DiscountType.Percentage);
            retrievedItem.Discount.Percentage.Value.Should().Be(10.0m);
        }
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotExist_ShouldReturnNull()
    {
        var dbName = Guid.NewGuid().ToString();

        await using var context = DbContextFactory.Create(dbName);
        var repository = new OrderRepository(context);
        var retrieved = await repository.GetByIdAsync(Guid.NewGuid());

        retrieved.Should().BeNull();
    }
}
