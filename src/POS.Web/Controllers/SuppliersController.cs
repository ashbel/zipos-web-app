using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Inventory.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _service;

    public SuppliersController(ISupplierService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> Search([FromQuery] string organizationId, [FromQuery] string? q, CancellationToken ct)
    {
        var items = await _service.SearchSuppliersAsync(organizationId, q, ct);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> Create([FromQuery] string organizationId, [FromBody] CreateSupplierRequest request, CancellationToken ct)
    {
        var s = await _service.CreateSupplierAsync(organizationId, request, ct);
        return Ok(s);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> Update([FromQuery] string organizationId, [FromRoute] string id, [FromBody] UpdateSupplierRequest request, CancellationToken ct)
    {
        var s = await _service.UpdateSupplierAsync(organizationId, id, request, ct);
        if (s == null) return NotFound();
        return Ok(s);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> Delete([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var ok = await _service.DeleteSupplierAsync(organizationId, id, ct);
        return ok ? NoContent() : NotFound();
    }
}

