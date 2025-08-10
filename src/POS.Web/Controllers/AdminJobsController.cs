using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Infrastructure.Services;
using POS.Shared.Infrastructure;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/admin/jobs")] 
public class AdminJobsController : ControllerBase
{
    private readonly IBackgroundJobService _jobs;
    private readonly StockAlertJob _stockAlertJob;
    private readonly OutOfDateTenantMigrationJob _migrationJob;

    public AdminJobsController(IBackgroundJobService jobs, StockAlertJob stockAlertJob, OutOfDateTenantMigrationJob migrationJob)
    {
        _jobs = jobs;
        _stockAlertJob = stockAlertJob;
        _migrationJob = migrationJob;
    }

    [HttpPost("stock-alerts/run")] 
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public IActionResult RunStockAlerts([FromQuery] string organizationId)
    {
        _jobs.Enqueue(() => _stockAlertJob.RunAsync(organizationId, default));
        return Accepted();
    }

    [HttpPost("migrations/run")] 
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public IActionResult RunMigrations([FromQuery] string organizationId)
    {
        _jobs.Enqueue(() => _migrationJob.RunAsync(organizationId, default));
        return Accepted();
    }
}


