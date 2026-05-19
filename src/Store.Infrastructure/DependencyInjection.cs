using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.Infrastructure.Persistence;

namespace Store.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StoreDatabase");

        services.AddDbContext<StoreDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHostedService<DatabaseMigrationHostedService>();

        return services;
    }
}
