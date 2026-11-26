using AutoFixture;
using FluentAssertions;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Application.Common;
using Lab.Orders.Application.Orders;
using Lab.Orders.Domain.Customers;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Orders;
using Lab.Orders.Domain.Products;
using Lab.Orders.Domain.Shared;
using NSubstitute;
using Xunit;

namespace Lab.Orders.Application.UnitTests.Orders;

public class RegisterOrderTests
{
    private const string ValidCpf = "52998224725";

    private readonly Fixture _fixture = new();
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly IIdempotencyStore _idempotencyStore = Substitute.For<IIdempotencyStore>();
    private readonly RegisterOrder _sut;

    private readonly Customer _customer;
    private readonly Product _product;

    public RegisterOrderTests()
    {
        _sut = new RegisterOrder(_customerRepository, _productRepository, _orderRepository, _idempotencyStore);

        _customer = Customer.Create(
            Guid.NewGuid(),
            _fixture.Create<string>(),
            _fixture.Create<string>(),
            Document.Create(ValidCpf),
            Address.Create("Rua A", "1", null, "Centro", "São Paulo", "SP", "01001000"));

        _product = Product.Create(Guid.NewGuid(), "SKU-1", _fixture.Create<string>(), new Money(10m));

        _customerRepository.GetByIdAsync(_customer.CustomerId, Arg.Any<CancellationToken>()).Returns(_customer);
        _productRepository.GetBySkuAsync("SKU-1", Arg.Any<CancellationToken>()).Returns(_product);
    }

    private RegisterOrderCommand ValidCommand(string idempotencyKey = "key-1") => new(
        _customer.CustomerId,
        [new OrderItemInput("SKU-1", 1, null)],
        PaymentMethod.Pix,
        null,
        0m,
        idempotencyKey);

    [Fact]
    public async Task ExecuteAsync_WithValidData_CreatesOrderAndSavesIdempotencyRecord()
    {
        Order? added = null;
        await _orderRepository.AddAsync(Arg.Do<Order>(o => added = o), Arg.Any<CancellationToken>());

        var result = await _sut.ExecuteAsync(ValidCommand());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Subtotal.Should().Be(10m);
        result.Value.Total.Should().Be(22m);
        added.Should().NotBeNull();
        await _idempotencyStore.Received(1).SaveAsync(
            Arg.Is<IdempotencyRecord>(r => r.Key == "key-1" && r.OrderId == added!.OrderId && r.PayloadHash.Length == 64),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingIdempotencyKey_ReturnsValidationFailure()
    {
        var result = await _sut.ExecuteAsync(ValidCommand(idempotencyKey: " "));

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Validation);
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownCustomer_ReturnsNotFound()
    {
        _customerRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Customer?)null);

        var result = await _sut.ExecuteAsync(ValidCommand());

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.NotFound);
        result.Failure.Code.Should().Be("CustomerNotFound");
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownProductSku_ReturnsNotFound()
    {
        _productRepository.GetBySkuAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _sut.ExecuteAsync(ValidCommand());

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.NotFound);
        result.Failure.Code.Should().Be("ProductNotFound");
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithOrderDiscountGreaterThanSubtotal_ReturnsValidationFailure()
    {
        var command = ValidCommand() with { Discount = new DiscountInput(DiscountType.Amount, 999m, null) };

        var result = await _sut.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Validation);
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithDistanceOver100Km_ReturnsValidationFailure()
    {
        var command = ValidCommand() with { DistanceKm = 100.5m };

        var result = await _sut.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Validation);
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithOrderAndItemDiscountsCombined_ReturnsValidationFailure()
    {
        var command = ValidCommand() with
        {
            Discount = new DiscountInput(DiscountType.Percentage, null, 10m),
            Items = [new OrderItemInput("SKU-1", 1, new DiscountInput(DiscountType.Percentage, null, 5m))]
        };

        var result = await _sut.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Validation);
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithDiscountHavingBothAmountAndPercentage_ReturnsValidationFailure()
    {
        var command = ValidCommand() with { Discount = new DiscountInput(DiscountType.Amount, 5m, 5m) };

        var result = await _sut.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Validation);
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SameKeyAndSamePayload_ReturnsSameOrderWithoutDuplicate()
    {
        Order? added = null;
        await _orderRepository.AddAsync(Arg.Do<Order>(o => added = o), Arg.Any<CancellationToken>());

        var command = ValidCommand("idem-key");

        var first = await _sut.ExecuteAsync(command);
        first.IsSuccess.Should().BeTrue();

        _idempotencyStore.GetAsync("idem-key", Arg.Any<CancellationToken>())
            .Returns(new IdempotencyRecord("idem-key", CaptureHash(), added!.OrderId, DateTimeOffset.UtcNow));
        _orderRepository.GetByIdAsync(added.OrderId, Arg.Any<CancellationToken>()).Returns(added);

        var second = await _sut.ExecuteAsync(command);

        second.IsSuccess.Should().BeTrue();
        second.Value!.OrderId.Should().Be(first.Value!.OrderId);
        await _orderRepository.Received(1).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
        await _idempotencyStore.Received(1).SaveAsync(Arg.Any<IdempotencyRecord>(), Arg.Any<CancellationToken>());
    }

    private string CaptureHash() =>
        _idempotencyStore.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IIdempotencyStore.SaveAsync))
            .Select(c => ((IdempotencyRecord)c.GetArguments()[0]!).PayloadHash)
            .Last();

    [Fact]
    public async Task ExecuteAsync_SameKeyDifferentPayload_ReturnsConflict()
    {
        _idempotencyStore.GetAsync("key-1", Arg.Any<CancellationToken>())
            .Returns(new IdempotencyRecord("key-1", new string('0', 64), Guid.NewGuid(), DateTimeOffset.UtcNow));

        var result = await _sut.ExecuteAsync(ValidCommand());

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Conflict);
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithNewIdempotencyKey_CreatesNewOrder()
    {
        Order? firstAdded = null;
        await _orderRepository.AddAsync(Arg.Do<Order>(o => firstAdded = o), Arg.Any<CancellationToken>());

        var first = await _sut.ExecuteAsync(ValidCommand("key-1"));
        var second = await _sut.ExecuteAsync(ValidCommand("key-2"));

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        second.Value!.OrderId.Should().NotBe(first.Value!.OrderId);
        await _orderRepository.Received(2).AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }
}
