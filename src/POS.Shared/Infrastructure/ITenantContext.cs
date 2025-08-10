namespace POS.Shared.Infrastructure;

public interface ITenantContext
{
    string? OrganizationId { get; }
    string? BranchId { get; }
    string? UserId { get; }
    void SetTenant(string organizationId, string? branchId = null, string? userId = null);
}