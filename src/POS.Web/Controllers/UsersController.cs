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

    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> Update([FromQuery] string organizationId, [FromRoute] string id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        _tenantContext.SetTenant(organizationId);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName!;
        if (!string.IsNullOrWhiteSpace(request.LastName)) user.LastName = request.LastName!;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;
        if (!string.IsNullOrWhiteSpace(request.Password)) user.PasswordHash = ComputeSha256(request.Password!);

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> Delete([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        _tenantContext.SetTenant(organizationId);
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("{id}/branches")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> AssignBranch([FromQuery] string organizationId, [FromRoute] string id, [FromBody] AssignBranchRequest request, CancellationToken ct)
    {
        _tenantContext.SetTenant(organizationId);
        var userExists = await _db.Users.AsNoTracking().AnyAsync(u => u.Id == id, ct);
        if (!userExists) return NotFound("User not found");
        var branchExists = await _db.Branches.AsNoTracking().AnyAsync(b => b.Id == request.BranchId, ct);
        if (!branchExists) return NotFound("Branch not found");

        var existing = await _db.UserBranches.FirstOrDefaultAsync(ub => ub.UserId == id && ub.BranchId == request.BranchId, ct);
        if (existing == null)
        {
            existing = new UserBranch { UserId = id, BranchId = request.BranchId, IsDefault = request.IsDefault };
            _db.UserBranches.Add(existing);
        }
        else
        {
            existing.IsDefault = request.IsDefault || existing.IsDefault;
        }

        if (request.IsDefault)
        {
            var others = _db.UserBranches.Where(ub => ub.UserId == id && ub.BranchId != request.BranchId);
            await others.ForEachAsync(ub => ub.IsDefault = false, ct);
        }

        await _db.SaveChangesAsync(ct);
        return Ok(new { userId = id, request.BranchId, request.IsDefault });
    }

    [HttpDelete("{id}/branches/{branchId}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> UnassignBranch([FromQuery] string organizationId, [FromRoute] string id, [FromRoute] string branchId, CancellationToken ct)
    {
        _tenantContext.SetTenant(organizationId);
        var assignment = await _db.UserBranches.FirstOrDefaultAsync(ub => ub.UserId == id && ub.BranchId == branchId, ct);
        if (assignment == null) return NotFound();
        _db.UserBranches.Remove(assignment);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPut("{id}/branches/default/{branchId}")]
    [Authorize(Policy = "CanManageUsers")]
    public async Task<IActionResult> SetDefaultBranch([FromQuery] string organizationId, [FromRoute] string id, [FromRoute] string branchId, CancellationToken ct)
    {
        _tenantContext.SetTenant(organizationId);
        var exists = await _db.UserBranches.AnyAsync(ub => ub.UserId == id && ub.BranchId == branchId, ct);
        if (!exists) return NotFound();

        var all = _db.UserBranches.Where(ub => ub.UserId == id);
        await all.ForEachAsync(ub => ub.IsDefault = ub.BranchId == branchId, ct);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    public record CreateUserRequest(string Email, string FirstName, string LastName, string Password);
    public record UpdateUserRequest(string? FirstName, string? LastName, bool? IsActive, string? Password);
    public record AssignBranchRequest(string BranchId, bool IsDefault = false);

    private static string ComputeSha256(string input)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(input);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

