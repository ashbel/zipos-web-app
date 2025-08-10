using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Inventory.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Search([FromQuery] string organizationId, [FromQuery] string? q, CancellationToken ct)
    {
        var items = await _productService.SearchProductsAsync(organizationId, q, ct);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Create([FromQuery] string organizationId, [FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = await _productService.CreateProductAsync(organizationId, request, ct);
        return Ok(product);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Update([FromQuery] string organizationId, [FromRoute] string id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var product = await _productService.UpdateProductAsync(organizationId, id, request, ct);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Delete([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var ok = await _productService.DeleteProductAsync(organizationId, id, ct);
        return ok ? NoContent() : NotFound();
    }
}

