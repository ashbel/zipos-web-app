using POS.Shared.Domain;

namespace POS.Modules.Sales.Services;

public interface ISalesService
{
    Task<Cart> CreateCartAsync(string organizationId, string userId, string branchId, CancellationToken ct = default);
    Task<Cart> AddItemAsync(string organizationId, string cartId, string productId, string name, decimal quantity, decimal unitPrice, decimal discount, CancellationToken ct = default);
    Task<Cart> ApplyPromotionsAsync(string organizationId, string cartId, string? promoCode, string? customerId, CancellationToken ct = default);
    Task<Cart> RemoveItemAsync(string organizationId, string cartId, string itemId, CancellationToken ct = default);
    Task<Sale> CheckoutAsync(string organizationId, string cartId, IEnumerable<PaymentRequest> payments, string? customerId = null, CancellationToken ct = default);
    Task<Refund> ProcessRefundAsync(string organizationId, string saleId, IEnumerable<RefundLineRequest> items, string reason, string processedBy, CancellationToken ct = default);
    Task<bool> ApproveRefundAsync(string organizationId, string refundId, string approvedBy, CancellationToken ct = default);
    Task<bool> RejectRefundAsync(string organizationId, string refundId, string rejectedBy, CancellationToken ct = default);
}

public record PaymentRequest(string Method, decimal Amount, string? Reference);
public record RefundLineRequest(string SaleItemId, string ProductId, decimal Quantity, bool Restock);

