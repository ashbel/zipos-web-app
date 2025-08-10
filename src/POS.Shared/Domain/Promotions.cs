namespace POS.Shared.Domain;

public class Promotion : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string Type { get; set; } = "PercentOff"; // PercentOff, AmountOff, BOGO, TierPercentOff
    public bool RequiresCode { get; set; } = false;
    public string? BranchId { get; set; }
    public string? ProductId { get; set; }
    public string? CategoryId { get; set; }
    public decimal? PercentOff { get; set; } // 0..100
    public decimal? AmountOff { get; set; } // per unit
    public int? BuyQuantity { get; set; }
    public int? GetFreeQuantity { get; set; }
    public decimal? MinCartTotal { get; set; }
    public int Priority { get; set; } = 0;
    public string? RequiredTier { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}


