using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Inventory.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrdersController(IPurchaseOrderService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> Create([FromQuery] string organizationId, [FromQuery] string supplierId, [FromQuery] string branchId, [FromQuery] string createdBy, CancellationToken ct)
    {
        var po = await _service.CreateAsync(organizationId, supplierId, branchId, createdBy, ct);
        return Ok(po);
    }

    [HttpPost("{poId}/lines")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)]
    public async Task<IActionResult> AddLine([FromQuery] string organizationId, [FromRoute] string poId, [FromQuery] string productId, [FromQuery] decimal quantity, [FromQuery] decimal unitCost, CancellationToken ct)
    {
        var line = await _service.AddLineAsync(organizationId, poId, productId, quantity, unitCost, ct);
        return Ok(line);
    }

    [HttpPost("{poId}/submit")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)]
    public async Task<IActionResult> Submit([FromQuery] string organizationId, [FromRoute] string poId, CancellationToken ct)
    {
        var ok = await _service.SubmitAsync(organizationId, poId, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{poId}/approve")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)]
    public async Task<IActionResult> Approve([FromQuery] string organizationId, [FromRoute] string poId, [FromQuery] string approvedBy, CancellationToken ct)
    {
        var ok = await _service.ApproveAsync(organizationId, poId, approvedBy, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{poId}/receive")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)]
    public async Task<IActionResult> Receive([FromQuery] string organizationId, [FromRoute] string poId, [FromQuery] string receivedBy, [FromBody] IEnumerable<ReceiveLineRequest> lines, CancellationToken ct)
    {
        var gr = await _service.ReceiveAsync(organizationId, poId, receivedBy, lines, ct);
        return Ok(gr);
    }

    [HttpPost("{poId}/close")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)]
    public async Task<IActionResult> Close([FromQuery] string organizationId, [FromRoute] string poId, CancellationToken ct)
    {
        var ok = await _service.CloseAsync(organizationId, poId, ct);
        return ok ? NoContent() : NotFound();
    }
}

