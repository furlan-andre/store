using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Store.Application.Common.Pagination;
using Store.Application.Orders;
using Store.Domain.Orders;
using Store.FunctionalTests.Infrastructure;

namespace Store.FunctionalTests.Orders;

[Collection(FunctionalTestCollection.Name)]
public sealed class OrdersFunctionalTests(FunctionalTestFixture fixture)
{
    [Fact]
    public async Task Create_ShouldPersistOrderAndCalculateTotal_WhenRequestIsValid()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/orders", CreateOrderRequest(
            (productId: 1, quantity: 2),
            (productId: 2, quantity: 3)));

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.Status.Should().Be(OrderStatus.Placed);
        order.Items.Should().HaveCount(2);
        order.Total.Should().Be(7450m);

        var getResponse = await client.GetFromJsonAsync<OrderResponse>($"/orders/{order.Id}");
        getResponse!.Total.Should().Be(7450m);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenOrderHasNoItems()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/orders", new CreateOrderRequest
        {
            CustomerId = 1,
            Currency = "BRL",
            Items = []
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenItemQuantityIsNotGreaterThanZero()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/orders", CreateOrderRequest((1, 0)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/orders", CreateOrderRequest((999, 1)));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturnUnprocessableEntity_WhenQuantityExceedsAvailableStock()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/orders", CreateOrderRequest((1, 11)));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Confirm_ShouldConfirmPlacedOrderAndDecreaseStock()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        var order = await CreateOrderAsync(client, CreateOrderRequest((1, 2)));

        var response = await client.PostAsync($"/orders/{order.Id}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var confirmedOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
        confirmedOrder!.Status.Should().Be(OrderStatus.Confirmed);
        confirmedOrder.ConfirmedAt.Should().NotBeNull();

        var product = await fixture.GetProductAsync(1);
        product.AvailableQuantity.Should().Be(8);
    }

    [Fact]
    public async Task Confirm_ShouldBeIdempotent_WhenEndpointIsCalledTwice()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        var order = await CreateOrderAsync(client, CreateOrderRequest((1, 2)));

        var firstResponse = await client.PostAsync($"/orders/{order.Id}/confirm", null);
        var secondResponse = await client.PostAsync($"/orders/{order.Id}/confirm", null);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstOrder = await firstResponse.Content.ReadFromJsonAsync<OrderResponse>();
        var secondOrder = await secondResponse.Content.ReadFromJsonAsync<OrderResponse>();
        secondOrder!.Id.Should().Be(firstOrder!.Id);
        secondOrder.Status.Should().Be(OrderStatus.Confirmed);
        secondOrder.Total.Should().Be(firstOrder.Total);
        secondOrder.ConfirmedAt.Should().BeCloseTo(firstOrder.ConfirmedAt!.Value, TimeSpan.FromMilliseconds(1));

        var product = await fixture.GetProductAsync(1);
        product.AvailableQuantity.Should().Be(8);
    }

    [Fact]
    public async Task Confirm_ShouldReturnUnprocessableEntity_WhenOrderIsCanceled()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        var order = await CreateOrderAsync(client, CreateOrderRequest((1, 2)));
        
        await client.PostAsync($"/orders/{order.Id}/cancel", null);

        var response = await client.PostAsync($"/orders/{order.Id}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Cancel_ShouldCancelPlacedOrderWithoutChangingStock()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        var order = await CreateOrderAsync(client, CreateOrderRequest((1, 2)));

        var response = await client.PostAsync($"/orders/{order.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var canceledOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
        
        canceledOrder!.Status.Should().Be(OrderStatus.Canceled);
        canceledOrder.CancelledAt.Should().NotBeNull();

        var product = await fixture.GetProductAsync(1);
        product.AvailableQuantity.Should().Be(10);
    }

    [Fact]
    public async Task Cancel_ShouldReturnStock_WhenOrderIsConfirmed()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        var order = await CreateOrderAsync(client, CreateOrderRequest((1, 2)));
        await client.PostAsync($"/orders/{order.Id}/confirm", null);

        var response = await client.PostAsync($"/orders/{order.Id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var product = await fixture.GetProductAsync(1);
        product.AvailableQuantity.Should().Be(10);
    }

    [Fact]
    public async Task Cancel_ShouldBeIdempotent_WhenEndpointIsCalledTwice()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        var order = await CreateOrderAsync(client, CreateOrderRequest((1, 2)));
        await client.PostAsync($"/orders/{order.Id}/confirm", null);

        var firstResponse = await client.PostAsync($"/orders/{order.Id}/cancel", null);
        var secondResponse = await client.PostAsync($"/orders/{order.Id}/cancel", null);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstOrder = await firstResponse.Content.ReadFromJsonAsync<OrderResponse>();
        var secondOrder = await secondResponse.Content.ReadFromJsonAsync<OrderResponse>();
        
        secondOrder!.Id.Should().Be(firstOrder!.Id);
        secondOrder.Status.Should().Be(OrderStatus.Canceled);
        secondOrder.Total.Should().Be(firstOrder.Total);
        secondOrder.CancelledAt.Should().BeCloseTo(firstOrder.CancelledAt!.Value, TimeSpan.FromMilliseconds(1));

        var product = await fixture.GetProductAsync(1);
        product.AvailableQuantity.Should().Be(10);
    }

    [Fact]
    public async Task GetById_ShouldReturnOrder_WhenOrderExists()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        var order = await CreateOrderAsync(client, CreateOrderRequest((1, 1)));

        var response = await client.GetAsync($"/orders/{order.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var foundOrder = await response.Content.ReadFromJsonAsync<OrderResponse>();
        foundOrder!.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/orders/999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ShouldFilterByCustomerId()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        var customerOneOrder = await CreateOrderAsync(client, CreateOrderRequest(1, (1, 1)));
        await CreateOrderAsync(client, CreateOrderRequest(2, (2, 1)));

        var response = await client.GetFromJsonAsync<PagedResponse<OrderResponse>>("/orders?customerId=1");

        response!.Items.Should().ContainSingle();
        response.Items[0].Id.Should().Be(customerOneOrder.Id);
    }

    [Fact]
    public async Task GetAll_ShouldFilterByStatus()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        var confirmedOrder = await CreateOrderAsync(client, CreateOrderRequest((1, 1)));
        await CreateOrderAsync(client, CreateOrderRequest((2, 1)));
        await client.PostAsync($"/orders/{confirmedOrder.Id}/confirm", null);

        var response = await client.GetFromJsonAsync<PagedResponse<OrderResponse>>("/orders?status=Confirmed");

        response!.Items.Should().ContainSingle();
        response.Items[0].Id.Should().Be(confirmedOrder.Id);
        response.Items[0].Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task GetAll_ShouldFilterByCreatedDateRange()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        
        var oldOrder = await CreateOrderAsync(client, CreateOrderRequest((1, 1)));
        var currentOrder = await CreateOrderAsync(client, CreateOrderRequest((2, 1)));
        var oldDate = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var currentDate = new DateTimeOffset(2026, 5, 20, 0, 0, 0, TimeSpan.Zero);
        
        await fixture.SetOrderDatesAsync(oldOrder.Id, createdAt: oldDate);
        await fixture.SetOrderDatesAsync(currentOrder.Id, createdAt: currentDate);

        var response = await client.GetFromJsonAsync<PagedResponse<OrderResponse>>(
            "/orders?createdFrom=2026-05-01T00:00:00Z&createdTo=2026-05-31T23:59:59Z");

        response!.Items.Should().ContainSingle();
        response.Items[0].Id.Should().Be(currentOrder.Id);
    }

    [Fact]
    public async Task GetAll_ShouldFilterByCancelledDateRange()
    {
        await fixture.ResetDatabaseAsync();
        
        using var client = await fixture.CreateAuthenticatedClientAsync();
        var oldCanceledOrder = await CreateOrderAsync(client, CreateOrderRequest((1, 1)));
        var currentCanceledOrder = await CreateOrderAsync(client, CreateOrderRequest((2, 1)));
        
        await client.PostAsync($"/orders/{oldCanceledOrder.Id}/cancel", null);
        await client.PostAsync($"/orders/{currentCanceledOrder.Id}/cancel", null);
        await fixture.SetOrderDatesAsync(
            oldCanceledOrder.Id,
            cancelledAt: new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero));
        
        await fixture.SetOrderDatesAsync(
            currentCanceledOrder.Id,
            cancelledAt: new DateTimeOffset(2026, 5, 20, 0, 0, 0, TimeSpan.Zero));

        var response = await client.GetFromJsonAsync<PagedResponse<OrderResponse>>(
            "/orders?cancelledFrom=2026-05-01T00:00:00Z&cancelledTo=2026-05-31T23:59:59Z");

        response!.Items.Should().ContainSingle();
        response.Items[0].Id.Should().Be(currentCanceledOrder.Id);
    }

    private static async Task<OrderResponse> CreateOrderAsync(
        HttpClient client,
        CreateOrderRequest request)
    {
        var response = await client.PostAsJsonAsync("/orders", request);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }

    private static CreateOrderRequest CreateOrderRequest(
        params (long productId, long quantity)[] items)
    {
        return CreateOrderRequest(1, items);
    }

    private static CreateOrderRequest CreateOrderRequest(
        long customerId,
        params (long productId, long quantity)[] items)
    {
        return new CreateOrderRequest
        {
            CustomerId = customerId,
            Currency = "BRL",
            Items = items
                .Select(item => new CreateOrderItemRequest
                {
                    ProductId = item.productId,
                    Quantity = item.quantity
                })
                .ToList()
        };
    }
}
