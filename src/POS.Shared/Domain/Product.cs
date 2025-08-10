namespace POS.Shared.Domain;

public class Product : TenantEntity
{
    public string SKU { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal Cost { get; set; }
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

