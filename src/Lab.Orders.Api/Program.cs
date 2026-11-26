using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lab.Orders.Api.Middleware;
using Lab.Orders.Application;
using Lab.Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.InitializeDatabase();

app.UseExceptionHandler();
app.MapHealthChecks("/health");

app.MapPost("/customers", async (
    Lab.Orders.Application.Customers.RegisterCustomerCommand command,
    Lab.Orders.Application.Customers.RegisterCustomer handler,
    CancellationToken cancellationToken) =>
{
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

    var result = await handler.ExecuteAsync(command, cancellationToken);

    if (result.IsFailure)
        return Results.Problem(
            statusCode: MapStatusCode(result.Failure!.Type),
            title: result.Failure.Code,
            detail: result.Failure.Message);

    return Results.Created($"/customers/{result.Value!.CustomerId}", result.Value);
})
.WithName("RegisterCustomer")
.Produces<Lab.Orders.Application.Customers.RegisterCustomerResult>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status422UnprocessableEntity);

app.MapPost("/products", async (
    Lab.Orders.Application.Products.RegisterProductCommand command,
    Lab.Orders.Application.Products.RegisterProduct handler,
    CancellationToken cancellationToken) =>
{
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

    var result = await handler.ExecuteAsync(command, cancellationToken);

    if (result.IsFailure)
        return Results.Problem(
            statusCode: MapStatusCode(result.Failure!.Type),
            title: result.Failure.Code,
            detail: result.Failure.Message);

    return Results.Created($"/products/{result.Value!.ProductId}", result.Value);
})
.WithName("RegisterProduct")
.Produces<Lab.Orders.Application.Products.RegisterProductResult>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status422UnprocessableEntity);

app.MapPost("/orders", async (
    HttpRequest request,
    Lab.Orders.Application.Orders.RegisterOrderCommand? command,
    Lab.Orders.Application.Orders.RegisterOrder handler,
    CancellationToken cancellationToken) =>
{
    CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

    if (command is null)
        return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "InvalidRequest", detail: "Request body is required.");

    if (!request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKeyValues) ||
        string.IsNullOrWhiteSpace(idempotencyKeyValues.FirstOrDefault()))
    {
        return Results.Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "IdempotencyKeyRequired",
            detail: "The 'Idempotency-Key' header is required.");
    }

    var idempotencyKey = idempotencyKeyValues.First()!.Trim();
    var boundCommand = command with { IdempotencyKey = idempotencyKey };

    var result = await handler.ExecuteAsync(boundCommand, cancellationToken);

    if (result.IsFailure)
        return Results.Problem(
            statusCode: MapStatusCode(result.Failure!.Type),
            title: result.Failure.Code,
            detail: result.Failure.Message);

    return Results.Created($"/orders/{result.Value!.OrderId}", result.Value);
})
.WithName("RegisterOrder")
.Produces<Lab.Orders.Application.Orders.RegisterOrderResult>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status400BadRequest)
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status409Conflict)
.ProducesProblem(StatusCodes.Status422UnprocessableEntity);

app.Run();

static int MapStatusCode(Lab.Orders.Application.Common.FailureType type) => type switch
{
    Lab.Orders.Application.Common.FailureType.Validation => StatusCodes.Status400BadRequest,
    Lab.Orders.Application.Common.FailureType.NotFound => StatusCodes.Status404NotFound,
    Lab.Orders.Application.Common.FailureType.Conflict => StatusCodes.Status409Conflict,
    Lab.Orders.Application.Common.FailureType.Domain => StatusCodes.Status422UnprocessableEntity,
    _ => StatusCodes.Status500InternalServerError
};

public partial class Program { }
