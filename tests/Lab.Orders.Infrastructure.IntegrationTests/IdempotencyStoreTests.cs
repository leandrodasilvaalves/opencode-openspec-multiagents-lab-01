using FluentAssertions;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Infrastructure.Repositories;
using Xunit;

namespace Lab.Orders.Infrastructure.IntegrationTests;

public class IdempotencyStoreTests
{
    [Fact]
    public async Task SaveAsync_And_GetAsync_ShouldPersistAndRetrieveIdempotencyRecord()
    {
        var dbName = Guid.NewGuid().ToString();
        var key = $"idemp-{Guid.NewGuid():N}";
        var orderId = Guid.NewGuid();
        var payloadHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
        var createdAt = DateTimeOffset.UtcNow;

        var record = new IdempotencyRecord(key, payloadHash, orderId, createdAt);

        await using (var context = DbContextFactory.Create(dbName))
        {
            var store = new IdempotencyStore(context);
            await store.SaveAsync(record);
        }

        await using (var context = DbContextFactory.Create(dbName))
        {
            var store = new IdempotencyStore(context);
            var retrieved = await store.GetAsync(key);

            retrieved.Should().NotBeNull();
            retrieved!.Key.Should().Be(key);
            retrieved.PayloadHash.Should().Be(payloadHash);
            retrieved.OrderId.Should().Be(orderId);
            retrieved.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
        }
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ShouldReturnNull()
    {
        var dbName = Guid.NewGuid().ToString();

        await using var context = DbContextFactory.Create(dbName);
        var store = new IdempotencyStore(context);
        var retrieved = await store.GetAsync("non-existent-key");

        retrieved.Should().BeNull();
    }
}
