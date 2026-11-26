using Microsoft.Extensions.DependencyInjection;

namespace Lab.Orders.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<Customers.RegisterCustomer>();
        services.AddScoped<Products.RegisterProduct>();
        services.AddScoped<Orders.RegisterOrder>();

        return services;
    }
}
