using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Branches.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    [Authorize(Policy = "CanManageUsers")] // reuse for now; later create branch-specific policy
    public async Task<IActionResult> List([FromQuery] string organizationId, CancellationToken ct)
    {
        var items = await _branchService.GetBranchesAsync(organizationId, ct);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageUsers")] // reuse for now
    public async Task<IActionResult> Create([FromQuery] string organizationId, [FromBody] CreateBranchRequest request, CancellationToken ct)
    {
        var item = await _branchService.CreateBranchAsync(organizationId, request, ct);
        return Ok(item);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageUsers")] // reuse for now
    public async Task<IActionResult> Update([FromQuery] string organizationId, [FromRoute] string id, [FromBody] UpdateBranchRequest request, CancellationToken ct)
    {
        var item = await _branchService.UpdateBranchAsync(organizationId, id, request, ct);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageUsers")] // reuse for now
    public async Task<IActionResult> Delete([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var ok = await _branchService.DeleteBranchAsync(organizationId, id, ct);
        return ok ? NoContent() : NotFound();
    }
}

