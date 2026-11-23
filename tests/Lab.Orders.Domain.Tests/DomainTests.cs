using FluentAssertions;
using Xunit;
using Lab.Orders.Domain;

namespace Lab.Orders.Domain.Tests;

public class DomainTests
{
    [Fact]
    public void Customer_Deve_Possuir_LimitedeCredito_NaoNegativo()
    {
        // Arrange
        var id = Guid.NewGuid();
        var limit = 1000m;

        // Act
        var customer = new Customer(id, limit);

        // Assert
        customer.CreditLimit.Should().Be(limit);
        customer.Id.Should().Be(id);
    }

    [Fact]
    public void Customer_Deve_Throw_Quando_LimitNegativo()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Customer(Guid.NewGuid(), -100m));
    }

    [Fact]
    public void OrderItem_Deve_Calcular_LineTotal_Sem_Desconto()
    {
        // Arrange
        var item = new OrderItem(50m, 3); // DiscountPercent default = 0

        // Act
        var total = item.LineTotal;

        // Assert
        total.Should().Be(150m); // 50 * 3 * (1 - 0/100) = 150
    }

    [Fact]
    public void OrderItem_Deve_Calcular_LineTotal_Com_Desconto()
    {
        // Arrange
        var item = new OrderItem(100m, 2, 10m); // 10% de desconto

        // Act
        var total = item.LineTotal;

        // Assert
        total.Should().Be(180m); // 100 * 2 * (1 - 10/100) = 200 * 0.9 = 180
    }

    [Fact]
    public void OrderItem_Deve_Throw_Quando_QuantidadeZero()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrderItem(10m, 0));
    }

    [Fact]
    public void OrderItem_Deve_Throw_Quando_UnitPriceNegativo()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrderItem(-10m, 1));
    }

    [Fact]
    public void OrderItem_Deve_Throw_Quando_DiscountPercentAcimaDe100()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new OrderItem(10m, 1, 101m));
    }

    [Fact]
    public void Order_Deve_Possuir_Itens_E_Cliente()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var item1 = new OrderItem(50m, 2);
        var item2 = new OrderItem(30m, 1);

        // Act
        var order = new Order(customerId, item1, item2);

        // Assert
        order.CustomerId.Should().Be(customerId);
        order.Items.Count.Should().Be(2);
        order.Total.Should().Be(130m); // 100 + 30
    }

    [Fact]
    public void Order_Deve_Throw_Quando_Sem_Itens()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new Order(Guid.NewGuid()));
    }

    [Fact]
    public void Order_Deve_Calcular_Total_Corretamente()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var item1 = new OrderItem(50m, 2); // 100
        var item2 = new OrderItem(25m, 3); // 75

        // Act
        var order = new Order(customerId, item1, item2);

        // Assert
        order.GetTotal().Should().Be(175m); // 100 + 75
    }
}
