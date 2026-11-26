using Lab.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lab.Orders.Infrastructure;

public static class DatabaseInitializer
{
    private const int MaxAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    public static void InitializeDatabase(this IHost host)
    {
        ArgumentNullException.ThrowIfNull(host);

        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        if (!context.Database.IsRelational())
            return;

        var logger = scope.ServiceProvider.GetService<ILogger<IHost>>();

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                ApplySchema(context);
                return;
            }
            catch (Exception exception) when (attempt < MaxAttempts)
            {
                logger?.LogWarning(
                    exception,
                    "Database initialization attempt {Attempt} of {MaxAttempts} failed; retrying in {RetryDelay}.",
                    attempt,
                    MaxAttempts,
                    RetryDelay);
                Thread.Sleep(RetryDelay);
            }
        }
    }

    private static void ApplySchema(OrdersDbContext context)
    {
        if (context.Database.GetMigrations().Any())
            context.Database.Migrate();
        else
            context.Database.EnsureCreated();
    }
}
