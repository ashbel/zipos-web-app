namespace POS.Shared.Domain;

public class Cart : TenantEntity
{
    public string UserId { get; set; } = string.Empty;
    public string BranchId { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class CartItem : TenantEntity
{
    public string CartId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class Sale : TenantEntity
{
    public string BranchId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Completed"; // Completed, Refunded, Voided
}

public class SaleItem : TenantEntity
{
    public string SaleId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public class Payment : TenantEntity
{
    public string SaleId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty; // Cash, Card, Mobile, etc.
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
    public string Status { get; set; } = "Captured"; // Captured, Voided, Refunded
}

