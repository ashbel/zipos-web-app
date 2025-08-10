using POS.Shared.Domain;

namespace POS.Modules.Promotions.Services;

public interface IPromotionService
{
    Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync(string organizationId, string branchId, DateTime at, CancellationToken ct = default);
    Task<ApplyPromotionsResult> ApplyPromotionsAsync(string organizationId, string branchId, List<CartItem> cartItems, string? promoCode, string? customerTier, CancellationToken ct = default);
}

public record ApplyPromotionsResult(List<CartItem> Items, decimal TotalDiscount, List<string> AppliedPromotions);

