using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Store.API.Authentication;
using Store.Application.Common.Results;
using Store.Application.Products;

namespace Store.Tests.API.Security;

public sealed class SecurityPipelineTests
{
    [Fact]
    public async Task ProtectedEndpoint_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        await using var factory = new StoreApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        await using var factory = new StoreApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/token", new LoginRequest
        {
            Username = "admin",
            Password = "admin"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var token = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        token.Should().NotBeNull();
        token!.AccessToken.Should().NotBeNullOrWhiteSpace();
        token.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        await using var factory = new StoreApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/auth/token", new LoginRequest
        {
            Username = "admin",
            Password = "invalid"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_ShouldReturnOk_WhenTokenIsValid()
    {
        await using var factory = new StoreApiFactory();
        using var client = factory.CreateClient();
        var token = await GetAccessTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<string> GetAccessTokenAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/auth/token", new LoginRequest
        {
            Username = "admin",
            Password = "admin"
        });

        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();

        return token!.AccessToken;
    }

    private sealed class StoreApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IHostedService>();
                services.RemoveAll<IProductService>();

                var productService = new Mock<IProductService>();
                productService
                    .Setup(service => service.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Result<IReadOnlyList<ProductResponse>>.Success([]));

                services.AddSingleton(productService.Object);
            });
        }
    }
}
