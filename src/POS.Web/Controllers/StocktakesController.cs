using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Inventory.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocktakesController : ControllerBase
{
    private readonly IStocktakeService _service;

    public StocktakesController(IStocktakeService service)
    {
        _service = service;
    }

    [HttpPost("start")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Start([FromQuery] string organizationId, [FromQuery] string branchId, [FromQuery] string startedBy, CancellationToken ct)
    {
        var session = await _service.StartSessionAsync(organizationId, branchId, startedBy, ct);
        return Ok(session);
    }

    [HttpPost("{sessionId}/count")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Count([FromQuery] string organizationId, [FromRoute] string sessionId, [FromQuery] string productId, [FromQuery] decimal countedQty, CancellationToken ct)
    {
        var line = await _service.UpsertCountAsync(organizationId, sessionId, productId, countedQty, ct);
        return Ok(line);
    }

    [HttpPost("{sessionId}/finalize")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Finalize([FromQuery] string organizationId, [FromRoute] string sessionId, [FromQuery] string finalizedBy, [FromQuery] bool createAdjustments, CancellationToken ct)
    {
        var session = await _service.FinalizeSessionAsync(organizationId, sessionId, finalizedBy, createAdjustments, ct);
        return Ok(session);
    }

    [HttpGet("{sessionId}/lines")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Lines([FromQuery] string organizationId, [FromRoute] string sessionId, CancellationToken ct)
    {
        var lines = await _service.GetSessionLinesAsync(organizationId, sessionId, ct);
        return Ok(lines);
    }
}

