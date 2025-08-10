using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Reporting.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IProfitabilityReportService _profit;

    public ReportsController(IProfitabilityReportService profit)
    {
        _profit = profit;
    }

    [HttpGet("profitability/summary")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder policy
    public async Task<IActionResult> ProfitSummary([FromQuery] string organizationId, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string? branchId, CancellationToken ct)
    {
        var r = await _profit.GetSummaryAsync(organizationId, from, to, branchId, ct);
        return Ok(r);
    }

    [HttpGet("profitability/products")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)]
    public async Task<IActionResult> ProfitByProduct([FromQuery] string organizationId, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string? branchId, CancellationToken ct)
    {
        var r = await _profit.GetProductProfitAsync(organizationId, from, to, branchId, ct);
        return Ok(r);
    }
}

