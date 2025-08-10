using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public UsersController(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> GetAll([FromQuery] string organizationId, CancellationToken ct)
    {
        _tenantContext.SetTenant(organizationId);
        var users = await _db.Users.AsNoTracking().Select(u => new { u.Id, u.Email, u.FirstName, u.LastName, u.IsActive }).ToListAsync(ct);
        return Ok(users);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> Create([FromQuery] string organizationId, [FromBody] CreateUserRequest request, CancellationToken ct)
    {
        _tenantContext.SetTenant(organizationId);
        var user = new User
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PasswordHash = ComputeSha256(request.Password),
            IsActive = true
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetAll), new { organizationId }, new { user.Id });
    }

    public record CreateUserRequest(string Email, string FirstName, string LastName, string Password);

    private static string ComputeSha256(string input)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

