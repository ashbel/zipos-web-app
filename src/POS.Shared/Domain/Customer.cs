namespace POS.Shared.Domain;

public class Customer : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string? LoyaltyTier { get; set; }
}

