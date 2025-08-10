using Microsoft.AspNetCore.Mvc;
using POS.Infrastructure.Data;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly POSDbContext _context;

    public HealthController(POSDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            // Test database connectivity
            await _context.Database.CanConnectAsync();
            
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Database = "Connected",
                Version = "1.0.0"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Database = "Disconnected",
                Error = ex.Message
            });
        }
    }
}