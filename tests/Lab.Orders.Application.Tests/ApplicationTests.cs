using FluentAssertions;
using Xunit;
using Lab.Orders.Application;
using Lab.Orders.Domain;

namespace Lab.Orders.Application.Tests;

public class CreateOrderUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ClienteInexistente_Deve_RetornarFalha()
    {
        // Arrange
        var mockRepo = new MockCustomerRepository(null!);
        var useCase = new CreateOrderUseCase(mockRepo, Mock.OrdersRepository());

        // Act
        var result = await useCase.ExecuteAsync(new CreateOrderCommand(Guid.NewGuid(), new()));

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Cliente não encontrado");
    }

    [Fact]
    public async Task ExecuteAsync_LimiteExcedido_Deve_RetornarFalha()
    {
        // Arrange
        var customer = new Customer(Guid.NewGuid(), 100m); // limite baixo
        var mockRepo = new MockCustomerRepository(customer);
        mockRepo.Orders = new List<Order> { };
        var useCase = new CreateOrderUseCase(mockRepo, Mock.OrdersRepository());

        // Act
        var result = await useCase.ExecuteAsync(new CreateOrderCommand(customer.Id, new()
        {
            new CreateOrderItemDto(150m, 1) // total 150 > limite 100
        }));

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Contain("Limite de crédito excedido");
    }

    [Fact]
    public async Task ExecuteAsync_Valido_Deve_RetornarSucesso()
    {
        // Arrange
        var customer = new Customer(Guid.NewGuid(), 1000m);
        var mockRepo = new MockCustomerRepository(customer);
        mockRepo.Orders = new List<Order> { };
        var useCase = new CreateOrderUseCase(mockRepo, Mock.OrdersRepository());

        // Act
        var result = await useCase.ExecuteAsync(new CreateOrderCommand(customer.Id, new()
        {
            new CreateOrderItemDto(100m, 1) // total 100 < limite 1000
        }));

        // Assert
        result.Success.Should().BeTrue();
        result.Order!.Total.Should().Be(100m);
    }
}
