using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Inventory.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Get([FromQuery] string organizationId, [FromQuery] string productId, [FromQuery] string branchId, CancellationToken ct)
    {
        var item = await _inventoryService.GetInventoryAsync(organizationId, productId, branchId, ct);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost("adjust")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Adjust([FromQuery] string organizationId, [FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        var item = await _inventoryService.AdjustStockAsync(organizationId, request, ct);
        return Ok(item);
    }

    [HttpPost("adjustments")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> RequestAdjustment([FromQuery] string organizationId, [FromBody] RequestAdjustmentRequest request, CancellationToken ct)
    {
        var adj = await _inventoryService.RequestAdjustmentAsync(organizationId, request, ct);
        return Ok(adj);
    }

    [HttpPost("adjustments/{id}/approve")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> ApproveAdjustment([FromQuery] string organizationId, [FromRoute] string id, [FromQuery] string approvedBy, CancellationToken ct)
    {
        var ok = await _inventoryService.ApproveAdjustmentAsync(organizationId, id, approvedBy, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("alerts")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Alerts([FromQuery] string organizationId, CancellationToken ct)
    {
        var items = await _inventoryService.GetStockAlertsAsync(organizationId, ct);
        return Ok(items);
    }

    [HttpPost("alerts/{id}/ack")] 
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> AckAlert([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var ok = await _inventoryService.AcknowledgeAlertAsync(organizationId, id, ct);
        return ok ? NoContent() : NotFound();
    }
}

