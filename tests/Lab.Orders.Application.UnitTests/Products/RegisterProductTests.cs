using AutoFixture;
using FluentAssertions;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Application.Common;
using Lab.Orders.Application.Products;
using Lab.Orders.Domain.Products;
using NSubstitute;
using Xunit;

namespace Lab.Orders.Application.UnitTests.Products;

public class RegisterProductTests
{
    private readonly Fixture _fixture = new();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly RegisterProduct _sut;

    public RegisterProductTests() => _sut = new RegisterProduct(_productRepository);

    [Fact]
    public async Task ExecuteAsync_WithValidData_ReturnsSuccessAndPersistsProduct()
    {
        var command = new RegisterProductCommand("SKU-1", _fixture.Create<string>(), 19.99m);

        var result = await _sut.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Sku.Should().Be("SKU-1");
        result.Value.BasePrice.Should().Be(19.99m);
        await _productRepository.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativePrice_ReturnsValidationFailure()
    {
        var command = new RegisterProductCommand("SKU-1", "Product", -0.01m);

        var result = await _sut.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Validation);
        await _productRepository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ExecuteAsync_WithMissingSku_ReturnsValidationFailure(string? sku)
    {
        var command = new RegisterProductCommand(sku, "Product", 10m);

        var result = await _sut.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Validation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task ExecuteAsync_WithMissingDescription_ReturnsValidationFailure(string? description)
    {
        var command = new RegisterProductCommand("SKU-1", description, 10m);

        var result = await _sut.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Validation);
    }
}
