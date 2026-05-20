using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Common.Http;
using Store.Application.Orders;

namespace Store.API.Controllers;

[ApiController]
[Authorize]
[Route("orders")]
public sealed class OrdersController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await orderService.CreateAsync(request, cancellationToken);

        return result.ToCreatedActionResult(result.IsSuccess ? $"/orders/{result.Value.Id}" : null);
    }

    [HttpPost("{id:long}/confirm")]
    public async Task<IActionResult> Confirm(long id, CancellationToken cancellationToken)
    {
        var result = await orderService.ConfirmAsync(id, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id, CancellationToken cancellationToken)
    {
        var result = await orderService.CancelAsync(id, cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ListOrdersRequest request,
        CancellationToken cancellationToken)
    {
        var result = await orderService.GetAllAsync(request, cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await orderService.GetByIdAsync(id, cancellationToken);

        return result.ToActionResult();
    }
}
