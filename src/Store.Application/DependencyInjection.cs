using Microsoft.Extensions.DependencyInjection;
using Store.Application.Customers;
using Store.Application.Products;

namespace Store.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IProductService, ProductService>();

        return services;
    }
}
