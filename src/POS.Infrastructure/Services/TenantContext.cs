using POS.Shared.Infrastructure;

namespace POS.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    private string? _organizationId;
    private string? _branchId;
    private string? _userId;

    public string? OrganizationId => _organizationId;
    public string? BranchId => _branchId;
    public string? UserId => _userId;

    public void SetTenant(string organizationId, string? branchId = null, string? userId = null)
    {
        _organizationId = organizationId;
        _branchId = branchId;
        _userId = userId;
    }
}