using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Lab.Orders.Application.Customers;
using Lab.Orders.Application.Orders;
using Lab.Orders.Application.Products;
using Lab.Orders.Domain.Discounts;
using Lab.Orders.Domain.Orders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Lab.Orders.Api.FunctionalTests;

public class OrdersApiFunctionalTests : IClassFixture<WebApplicationFactory<Program>>
{
    private sealed record CustomerResponse(Guid CustomerId, string Name);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client;

    public OrdersApiFunctionalTests(WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["UseInMemoryDatabase"] = "true",
                    ["DatabaseProvider"] = "InMemory"
                })))
            .CreateClient();
    }

    [Fact]
    public async Task Should_Register_Customer_Successfully()
    {
        var command = new RegisterCustomerCommand(
            "John Doe",
            "11999999999",
            "11144477735",
            "Main Street",
            "123",
            "Apt 4B",
            "Downtown",
            "Sao Paulo",
            "SP",
            "01001000");

        var response = await _client.PostAsJsonAsync("/customers", command, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        result.Should().NotBeNull();
        result!.CustomerId.Should().NotBeEmpty();
        result.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_Customer_Document_Is_Invalid()
    {
        var command = new RegisterCustomerCommand(
            "John Doe",
            "11999999999",
            "12345678901",
            "Main Street",
            "123",
            null,
            "Downtown",
            "Sao Paulo",
            "SP",
            "01001000");

        var response = await _client.PostAsJsonAsync("/customers", command, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
        using var problem = await response.Content.ReadFromJsonAsync<JsonDocument>(JsonOptions);
        problem.Should().NotBeNull();
        problem!.RootElement.GetProperty("title").GetString().Should().Be("InvalidCustomer");
        problem.RootElement.GetProperty("status").GetInt32().Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Should_Register_Product_Successfully()
    {
        var sku = $"SKU-{Guid.NewGuid():N}";
        var command = new RegisterProductCommand(sku, "Test Product", 50.00m);

        var response = await _client.PostAsJsonAsync("/products", command, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<RegisterProductResult>(JsonOptions);
        result.Should().NotBeNull();
        result!.ProductId.Should().NotBeEmpty();
        result.Sku.Should().Be(sku);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_Product_Price_Is_Negative()
    {
        var sku = $"SKU-{Guid.NewGuid():N}";
        var command = new RegisterProductCommand(sku, "Test Product", -10.00m);

        var response = await _client.PostAsJsonAsync("/products", command, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Should_Register_Order_Successfully_With_Idempotency()
    {
        var customerCommand = new RegisterCustomerCommand(
            "Jane Doe",
            "11988888888",
            "52998224725",
            "Second Street",
            "456",
            null,
            "Uptown",
            "Sao Paulo",
            "SP",
            "01310100");

        var customerResponse = await _client.PostAsJsonAsync("/customers", customerCommand, JsonOptions);
        customerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);

        var sku = $"SKU-{Guid.NewGuid():N}";
        var productCommand = new RegisterProductCommand(sku, "Order Product", 100.00m);
        await _client.PostAsJsonAsync("/products", productCommand, JsonOptions);

        var orderCommand = new RegisterOrderCommand(
            customer!.CustomerId,
            [new OrderItemInput(sku, 2, null)],
            PaymentMethod.Pix,
            null,
            10.00m,
            null);

        var idempotencyKey = Guid.NewGuid().ToString();
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/orders")
        {
            Content = JsonContent.Create(orderCommand, options: JsonOptions)
        };
        requestMessage.Headers.Add("Idempotency-Key", idempotencyKey);

        var response1 = await _client.SendAsync(requestMessage);
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        var orderResult1 = await response1.Content.ReadFromJsonAsync<RegisterOrderResult>(JsonOptions);

        var requestMessage2 = new HttpRequestMessage(HttpMethod.Post, "/orders")
        {
            Content = JsonContent.Create(orderCommand, options: JsonOptions)
        };
        requestMessage2.Headers.Add("Idempotency-Key", idempotencyKey);

        var response2 = await _client.SendAsync(requestMessage2);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);
        var orderResult2 = await response2.Content.ReadFromJsonAsync<RegisterOrderResult>(JsonOptions);

        orderResult2!.OrderId.Should().Be(orderResult1!.OrderId);
    }

    [Fact]
    public async Task Should_Return_BadRequest_When_Idempotency_Key_Header_Is_Missing()
    {
        var orderCommand = new RegisterOrderCommand(
            Guid.NewGuid(),
            [],
            PaymentMethod.Cash,
            null,
            5.00m,
            null);

        var response = await _client.PostAsJsonAsync("/orders", orderCommand, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
