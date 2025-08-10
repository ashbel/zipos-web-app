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
}

