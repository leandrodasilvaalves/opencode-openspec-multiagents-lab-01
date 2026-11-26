using FluentAssertions;
using Lab.Orders.Domain.Products;
using Lab.Orders.Domain.Shared;
using Lab.Orders.Infrastructure.Repositories;
using Xunit;

namespace Lab.Orders.Infrastructure.IntegrationTests;

public class ProductRepositoryTests
{
    [Fact]
    public async Task AddAsync_And_GetBySkuAsync_ShouldPersistAndRetrieveProduct()
    {
        var dbName = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid();
        var sku = "TECH-001";
        var product = Product.Create(productId, sku, "Mechanical Keyboard", new Money(149.90m));

        await using (var context = DbContextFactory.Create(dbName))
        {
            var repository = new ProductRepository(context);
            await repository.AddAsync(product);
        }

        await using (var context = DbContextFactory.Create(dbName))
        {
            var repository = new ProductRepository(context);
            var retrieved = await repository.GetBySkuAsync(sku);

            retrieved.Should().NotBeNull();
            retrieved!.ProductId.Should().Be(productId);
            retrieved.Sku.Should().Be(sku);
            retrieved.Description.Should().Be("Mechanical Keyboard");
            retrieved.BasePrice.Amount.Should().Be(149.90m);
        }
    }

    [Fact]
    public async Task GetBySkuAsync_ShouldTrimInputSku()
    {
        var dbName = Guid.NewGuid().ToString();
        var product = Product.Create(Guid.NewGuid(), "MOUSE-01", "Gaming Mouse", new Money(79.00m));

        await using (var context = DbContextFactory.Create(dbName))
        {
            var repository = new ProductRepository(context);
            await repository.AddAsync(product);
        }

        await using (var context = DbContextFactory.Create(dbName))
        {
            var repository = new ProductRepository(context);
            var retrieved = await repository.GetBySkuAsync("  MOUSE-01  ");

            retrieved.Should().NotBeNull();
            retrieved!.Sku.Should().Be("MOUSE-01");
        }
    }

    [Fact]
    public async Task SkuExistsAsync_WhenExists_ShouldReturnTrue()
    {
        var dbName = Guid.NewGuid().ToString();
        var product = Product.Create(Guid.NewGuid(), "HEADSET-01", "USB Headset", new Money(99.00m));

        await using (var context = DbContextFactory.Create(dbName))
        {
            var repository = new ProductRepository(context);
            await repository.AddAsync(product);
        }

        await using (var context = DbContextFactory.Create(dbName))
        {
            var repository = new ProductRepository(context);
            var exists = await repository.SkuExistsAsync("HEADSET-01");

            exists.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SkuExistsAsync_WhenNotExists_ShouldReturnFalse()
    {
        var dbName = Guid.NewGuid().ToString();

        await using var context = DbContextFactory.Create(dbName);
        var repository = new ProductRepository(context);
        var exists = await repository.SkuExistsAsync("NON-EXISTENT");

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetBySkuAsync_WhenNotExists_ShouldReturnNull()
    {
        var dbName = Guid.NewGuid().ToString();

        await using var context = DbContextFactory.Create(dbName);
        var repository = new ProductRepository(context);
        var retrieved = await repository.GetBySkuAsync("NON-EXISTENT");

        retrieved.Should().BeNull();
    }
}
