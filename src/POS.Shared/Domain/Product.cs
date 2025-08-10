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

