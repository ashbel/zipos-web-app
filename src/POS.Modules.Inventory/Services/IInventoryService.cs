using POS.Shared.Domain;

namespace POS.Modules.Inventory.Services;

public interface IInventoryService
{
    Task<InventoryItem> AdjustStockAsync(string organizationId, AdjustStockRequest request, CancellationToken ct = default);
    Task<InventoryItem?> GetInventoryAsync(string organizationId, string productId, string branchId, CancellationToken ct = default);
    Task<IReadOnlyList<StockAlert>> GetStockAlertsAsync(string organizationId, CancellationToken ct = default);
    Task<bool> AcknowledgeAlertAsync(string organizationId, string alertId, CancellationToken ct = default);
    Task<StockAdjustment> RequestAdjustmentAsync(string organizationId, RequestAdjustmentRequest request, CancellationToken ct = default);
    Task<bool> ApproveAdjustmentAsync(string organizationId, string adjustmentId, string approvedBy, CancellationToken ct = default);
}

public record AdjustStockRequest(string ProductId, string BranchId, decimal QuantityDelta, string Reason);
public record RequestAdjustmentRequest(string ProductId, string BranchId, decimal QuantityDelta, string Reason, string RequestedBy);

