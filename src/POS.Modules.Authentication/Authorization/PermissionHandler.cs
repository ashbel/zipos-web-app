using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Infrastructure;

namespace POS.Modules.Authentication.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public PermissionHandler(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var userId = context.User?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        var orgId = context.User?.FindFirst("org")?.Value;
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(orgId))
        {
            return;
        }

        _tenantContext.SetTenant(orgId);

        var hasPermission = await _db.RolePermissions
            .AsNoTracking()
            .Where(rp => _db.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == rp.RoleId))
            .AnyAsync(rp => _db.Permissions.Any(p => p.Id == rp.PermissionId && p.Name == requirement.Permission));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}

