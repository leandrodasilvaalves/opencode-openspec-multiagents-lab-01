using FluentAssertions;
using Lab.Orders.Domain.Products;
using Lab.Orders.Domain.Shared;
using Lab.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Lab.Orders.Infrastructure.IntegrationTests;

public class DatabaseInitializerTests
{
    [Fact]
    public void InitializeDatabase_WithInMemoryProvider_ShouldNotThrowAndKeepDatabaseUsable()
    {
        var services = new ServiceCollection();
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        using var host = new TestHost(scope.ServiceProvider);

        var initialize = () => host.InitializeDatabase();

        initialize.Should().NotThrow();

        var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        context.Products.Add(Product.Create(Guid.NewGuid(), "SCHEMA-001", "Schema Product", new Money(10.00m)));
        context.SaveChanges();

        context.Products.Should().HaveCount(1);
    }

    [Fact]
    public void MySqlModel_ShouldGenerateCreateScriptForAllMappedTables()
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseMySQL("Server=localhost;Port=3307;Database=ordersdb;Uid=root;Pwd=root;")
            .Options;

        using var context = new OrdersDbContext(options);

        var script = context.Database.GenerateCreateScript();

        script.Should().Contain("CREATE TABLE `Customers`");
        script.Should().Contain("CREATE TABLE `Products`");
        script.Should().Contain("CREATE TABLE `Orders`");
        script.Should().Contain("CREATE TABLE `OrderItems`");
        script.Should().Contain("CREATE TABLE `IdempotencyRecords`");
        script.Should().Contain("`PostalCode`");
        script.Should().Contain("IX_Products_Sku");
    }

    private sealed class TestHost(IServiceProvider services) : IHost
    {
        public IServiceProvider Services { get; } = services;

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
