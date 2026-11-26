using Microsoft.EntityFrameworkCore;
using Lab.Orders.Infrastructure.Persistence;

namespace Lab.Orders.Infrastructure.IntegrationTests;

internal static class DbContextFactory
{
    public static OrdersDbContext Create(string databaseName)
    {
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new OrdersDbContext(options);
    }
}
