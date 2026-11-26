using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lab.Orders.Application.Abstractions;
using Lab.Orders.Infrastructure.Persistence;
using Lab.Orders.Infrastructure.Repositories;

namespace Lab.Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var useInMemory = (bool.TryParse(configuration["UseInMemoryDatabase"], out var inMem) && inMem)
            || string.Equals(configuration["DatabaseProvider"], "InMemory", StringComparison.OrdinalIgnoreCase)
            || AppDomain.CurrentDomain.FriendlyName.Contains("testhost", StringComparison.OrdinalIgnoreCase);

        services.AddDbContext<OrdersDbContext>(options =>
        {
            if (useInMemory)
            {
                options.UseInMemoryDatabase("OrdersDb");
            }
            else
            {
                options.UseMySQL(connectionString ?? "Server=localhost;Database=ordersdb;Uid=root;Pwd=root;");
            }
        });

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();

        return services;
    }
}
