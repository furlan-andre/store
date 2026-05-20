using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Store.API.Authentication;
using Store.Domain.Customers;
using Store.Domain.Products;
using Store.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace Store.FunctionalTests.Infrastructure;

public sealed class FunctionalTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("store_tests")
        .WithUsername("store")
        .WithPassword("store")
        .Build();

    private StoreApiFactory? _factory;

    public StoreApiFactory Factory =>
        _factory ?? throw new InvalidOperationException("Test factory was not initialized.");

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        _factory = new StoreApiFactory(_postgres.GetConnectionString());
        Factory.CreateClient().Dispose();
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/token", new LoginRequest
        {
            Username = "admin",
            Password = "admin"
        });

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        return client;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();

        await dbContext.Database.ExecuteSqlRawAsync("""
            TRUNCATE TABLE order_items, orders, products, customers
            RESTART IDENTITY CASCADE;
            """);

        dbContext.Customers.AddRange(
            new Customer("Customer One"),
            new Customer("Customer Two"));

        dbContext.Products.AddRange(
            new Product("Notebook", 3500m, 10),
            new Product("Keyboard", 150m, 50),
            new Product("Mouse", 80m, 100));

        await dbContext.SaveChangesAsync();
    }

    public async Task<Product> GetProductAsync(long productId)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();

        return await dbContext.Products
            .AsNoTracking()
            .SingleAsync(product => product.Id == productId);
    }

    public async Task SetOrderDatesAsync(
        long orderId,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? cancelledAt = null)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext>();

        if (createdAt.HasValue)
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""UPDATE orders SET "CreatedAt" = {createdAt.Value} WHERE "Id" = {orderId};""");
        }

        if (cancelledAt.HasValue)
        {
            await dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""UPDATE orders SET "CancelledAt" = {cancelledAt.Value} WHERE "Id" = {orderId};""");
        }
    }

    public sealed class StoreApiFactory(string connectionString) : WebApplicationFactory<global::Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:StoreDatabase"] = connectionString,
                    ["Jwt:Issuer"] = "Store.API",
                    ["Jwt:Audience"] = "Store.API",
                    ["Jwt:Secret"] = "D8qX7mR2vK9pL4sT6nB3yW5zC1fH0jAe",
                    ["Jwt:ExpirationMinutes"] = "60",
                    ["Authentication:Username"] = "admin",
                    ["Authentication:Password"] = "admin"
                });
            });
        }
    }
}
