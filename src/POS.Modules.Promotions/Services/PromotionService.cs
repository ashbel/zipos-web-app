using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Promotions.Services;

public class PromotionService : IPromotionService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public PromotionService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(string organizationId, string branchId, DateTime at, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var q = _db.Set<Promotion>().AsNoTracking().Where(p => p.IsActive && p.StartDate <= at && (p.EndDate == null || p.EndDate >= at) && (p.BranchId == null || p.BranchId == branchId));
        return await q.OrderByDescending(p => p.Priority).ToListAsync(ct);
    }

    public async Task<ApplyPromotionsResult> ApplyPromotionsAsync(string organizationId, string branchId, List<CartItem> cartItems, string? promoCode, string? customerTier, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var now = DateTime.UtcNow;
        var promos = await GetActivePromotionsAsync(organizationId, branchId, now, ct);
        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            promos = promos.Where(p => p.RequiresCode && string.Equals(p.Code, promoCode, StringComparison.OrdinalIgnoreCase)).Concat(promos.Where(p => !p.RequiresCode)).ToList();
        }

        var applied = new List<string>();
        decimal totalDiscount = 0m;

        foreach (var promo in promos)
        {
            if (!string.IsNullOrWhiteSpace(promo.RequiredTier) && !string.Equals(promo.RequiredTier, customerTier, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            switch (promo.Type)
            {
                case "PercentOff":
                    if (promo.PercentOff is > 0)
                    {
                        foreach (var item in cartItems)
                        {
                            if (MatchesTarget(promo, item))
                            {
                                var discount = Math.Round((item.UnitPrice * item.Quantity) * (promo.PercentOff!.Value / 100m), 2);
                                if (discount > 0)
                                {
                                    item.DiscountAmount += discount;
                                    item.TotalAmount = (item.UnitPrice * item.Quantity) - item.DiscountAmount;
                                    totalDiscount += discount;
                                }
                            }
                        }
                        applied.Add(promo.Name);
                    }
                    break;
                case "AmountOff":
                    if (promo.AmountOff is > 0)
                    {
                        foreach (var item in cartItems)
                        {
                            if (MatchesTarget(promo, item))
                            {
                                var discount = Math.Round(promo.AmountOff!.Value * item.Quantity, 2);
                                item.DiscountAmount += discount;
                                item.TotalAmount = (item.UnitPrice * item.Quantity) - item.DiscountAmount;
                                totalDiscount += discount;
                            }
                        }
                        applied.Add(promo.Name);
                    }
                    break;
                case "BOGO":
                    if (promo.BuyQuantity is > 0 && promo.GetFreeQuantity is > 0)
                    {
                        foreach (var item in cartItems)
                        {
                            if (MatchesTarget(promo, item))
                            {
                                var groupSize = promo.BuyQuantity.Value + promo.GetFreeQuantity.Value;
                                var sets = (int)(item.Quantity / groupSize);
                                if (sets > 0)
                                {
                                    var freeQty = sets * promo.GetFreeQuantity.Value;
                                    var discount = Math.Round(freeQty * item.UnitPrice, 2);
                                    item.DiscountAmount += discount;
                                    item.TotalAmount = (item.UnitPrice * item.Quantity) - item.DiscountAmount;
                                    totalDiscount += discount;
                                }
                            }
                        }
                        applied.Add(promo.Name);
                    }
                    break;
                case "TierPercentOff":
                    if (!string.IsNullOrWhiteSpace(promo.RequiredTier) && promo.PercentOff is > 0 && string.Equals(promo.RequiredTier, customerTier, StringComparison.OrdinalIgnoreCase))
                    {
                        foreach (var item in cartItems)
                        {
                            if (MatchesTarget(promo, item))
                            {
                                var discount = Math.Round((item.UnitPrice * item.Quantity) * (promo.PercentOff!.Value / 100m), 2);
                                item.DiscountAmount += discount;
                                item.TotalAmount = (item.UnitPrice * item.Quantity) - item.DiscountAmount;
                                totalDiscount += discount;
                            }
                        }
                        applied.Add(promo.Name);
                    }
                    break;
            }
        }

        return new ApplyPromotionsResult(cartItems, totalDiscount, applied);
    }

    private static bool MatchesTarget(Promotion promo, CartItem item)
    {
        // For simplicity, match by product only when provided. Category support would require item category in CartItem.
        if (!string.IsNullOrWhiteSpace(promo.ProductId)) return string.Equals(promo.ProductId, item.ProductId, StringComparison.OrdinalIgnoreCase);
        return true;
    }
}


