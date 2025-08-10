namespace POS.Shared.Domain;

public class Supplier : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

public class PurchaseOrder : TenantEntity
{
    public string SupplierId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Receiving, Closed, Cancelled
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class PurchaseOrderLine : TenantEntity
{
    public string PurchaseOrderId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public decimal QuantityOrdered { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
}

public class GoodsReceipt : TenantEntity
{
    public string PurchaseOrderId { get; set; } = string.Empty;
    public string ReceivedBy { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}

public class GoodsReceiptLine : TenantEntity
{
    public string GoodsReceiptId { get; set; } = string.Empty;
    public string PurchaseOrderLineId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public decimal QuantityReceived { get; set; }
    public decimal UnitCost { get; set; }
}

public class PurchaseReturn : TenantEntity
{
    public string SupplierId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Submitted"; // Submitted, Approved, Closed
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PurchaseReturnLine : TenantEntity
{
    public string PurchaseReturnId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class SupplierProductPrice : TenantEntity
{
    public string SupplierId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public decimal LastPrice { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

