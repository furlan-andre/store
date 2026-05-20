using Microsoft.AspNetCore.Mvc;
using Store.API.Common.Http;
using Store.Application.Orders;

namespace Store.API.Controllers;

[ApiController]
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

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await orderService.GetAllAsync(cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await orderService.GetByIdAsync(id, cancellationToken);

        return result.ToActionResult();
    }
}
