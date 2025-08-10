using POS.Shared.Domain;

namespace POS.Modules.Inventory.Services;

public interface IInventoryService
{
    Task<InventoryItem> AdjustStockAsync(string organizationId, AdjustStockRequest request, CancellationToken ct = default);
    Task<InventoryItem?> GetInventoryAsync(string organizationId, string productId, string branchId, CancellationToken ct = default);
}

public record AdjustStockRequest(string ProductId, string BranchId, decimal QuantityDelta, string Reason);

