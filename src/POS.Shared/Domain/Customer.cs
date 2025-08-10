namespace POS.Shared.Domain;

public class Customer : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string? LoyaltyTier { get; set; }
    public bool IsActive { get; set; }
}

public class CustomerLoyalty : TenantEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public int Points { get; set; }
    public string Tier { get; set; } = "Basic";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class LoyaltyTierDefinition : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public int MinPoints { get; set; }
    public int? MaxPoints { get; set; }
    public decimal DiscountPercent { get; set; } // optional usage in pricing/promotions
    public int Priority { get; set; } = 0; // higher wins when overlapping
}

public class CustomerCredit : TenantEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal OutstandingBalance { get; set; }
    public string Status { get; set; } = "Active"; // Active, Suspended, Closed
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class CustomerCreditTransaction : TenantEntity
{
    public string CustomerId { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = "Charge"; // Charge, Payment, Adjustment
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public string? Note { get; set; }
    public string? SaleId { get; set; }
}

