using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Inventory.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseReturnsController : ControllerBase
{
    private readonly IPurchaseReturnService _service;

    public PurchaseReturnsController(IPurchaseReturnService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)]
    public async Task<IActionResult> Create([FromQuery] string organizationId, [FromQuery] string supplierId, [FromQuery] string branchId, [FromQuery] string reason, [FromQuery] string createdBy, [FromBody] IEnumerable<PurchaseReturnLineRequest> lines, CancellationToken ct)
    {
        var pr = await _service.CreateAsync(organizationId, supplierId, branchId, reason, createdBy, lines, ct);
        return Ok(pr);
    }

    [HttpPost("{id}/approve")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)]
    public async Task<IActionResult> Approve([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var ok = await _service.ApproveAsync(organizationId, id, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id}/close")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)]
    public async Task<IActionResult> Close([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var ok = await _service.CloseAsync(organizationId, id, ct);
        return ok ? NoContent() : NotFound();
    }
}

