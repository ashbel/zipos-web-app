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

public class CustomerLoyalty : TenantEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Tier { get; set; } = "Basic";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

