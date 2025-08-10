namespace POS.Shared.Domain;

public class Product : TenantEntity
{
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal Cost { get; set; }
    public string? CategoryId { get; set; }
    public string Attributes { get; set; } = "{}";
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
}

public class InventoryItem : TenantEntity
{
    public string ProductId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal AverageCost { get; set; }
    public decimal LastPurchasePrice { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class StockAlert : TenantEntity
{
    public string ProductId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
}

public class Category : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class StockMovement : TenantEntity
{
    public string ProductId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public decimal QuantityDelta { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
}

public class StockAdjustment : TenantEntity
{
    public string ProductId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public decimal QuantityDelta { get; set; }
    public string Reason { get; set; } = string.Empty; // e.g., Stocktake variance, Damage, Correction
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public string RequestedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class StocktakeSession : TenantEntity
{
    public string BranchId { get; set; } = string.Empty;
    public string Status { get; set; } = "Open"; // Open, Finalized
    public string StartedBy { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public string? FinalizedBy { get; set; }
    public DateTime? FinalizedAt { get; set; }
}

public class StocktakeLine : TenantEntity
{
    public string SessionId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public decimal ExpectedQty { get; set; }
    public decimal CountedQty { get; set; }
    public decimal VarianceQty { get; set; }
}

