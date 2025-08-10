namespace POS.Shared.Domain;

public abstract class TenantEntity : BaseEntity
{
    // OrganizationId is no longer needed since schema isolation handles tenant separation
    // Each tenant gets their own schema (e.g., org_12345) providing complete data isolation
}