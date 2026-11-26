using AutoFixture;
using FluentAssertions;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Application.Common;
using Lab.Orders.Application.Customers;
using Lab.Orders.Domain.Customers;
using NSubstitute;
using Xunit;

namespace Lab.Orders.Application.UnitTests.Customers;

public class RegisterCustomerTests
{
    private const string ValidCpf = "52998224725";

    private readonly Fixture _fixture = new();
    private readonly ICustomerRepository _customerRepository = Substitute.For<ICustomerRepository>();
    private readonly RegisterCustomer _sut;

    public RegisterCustomerTests() => _sut = new RegisterCustomer(_customerRepository);

    private RegisterCustomerCommand ValidCommand() => new(
        _fixture.Create<string>(),
        _fixture.Create<string>(),
        ValidCpf,
        "Rua A",
        "100",
        null,
        "Centro",
        "São Paulo",
        "SP",
        "01001-000");

    [Fact]
    public async Task ExecuteAsync_WithValidData_ReturnsSuccessAndPersistsCustomer()
    {
        var command = ValidCommand();

        var result = await _sut.ExecuteAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CustomerId.Should().NotBeEmpty();
        result.Value.Document.Should().Be(ValidCpf);
        result.Value.DocumentKind.Should().Be(DocumentKind.Cpf);
        await _customerRepository.Received(1).AddAsync(
            Arg.Is<Customer>(c => c.Document.Value == ValidCpf),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("52998224724")]
    [InlineData("abcdefghijk")]
    public async Task ExecuteAsync_WithInvalidDocument_ReturnsValidationFailure(string document)
    {
        var command = ValidCommand() with { Document = document };

        var result = await _sut.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Validation);
        await _customerRepository.DidNotReceive().AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingName_ReturnsValidationFailure()
    {
        var command = ValidCommand() with { Name = " " };

        var result = await _sut.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Failure!.Type.Should().Be(FailureType.Validation);
    }
}
