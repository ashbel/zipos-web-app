namespace POS.Shared.Domain;

public abstract class BranchEntity : TenantEntity
{
    public string BranchId { get; set; } = string.Empty;
}