using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Common.Http;
using Store.Application.Products;

namespace Store.API.Controllers;

[ApiController]
[Authorize]
[Route("products")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await productService.GetAllAsync(cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var result = await productService.GetByIdAsync(id, cancellationToken);

        return result.ToActionResult();
    }
}
