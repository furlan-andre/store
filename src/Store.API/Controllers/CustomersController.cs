using Microsoft.AspNetCore.Mvc;
using Store.API.Common.Http;
using Store.Application.Customers;

namespace Store.API.Controllers;

[ApiController]
[Route("customers")]
public sealed class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await customerService.GetAllAsync(cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await customerService.GetByIdAsync(id, cancellationToken);

        return result.ToActionResult();
    }
}
