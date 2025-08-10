using POS.Shared.Domain;

namespace POS.Modules.Inventory.Services;

public interface IPurchaseReturnService
{
    Task<PurchaseReturn> CreateAsync(string organizationId, string supplierId, string branchId, string reason, string createdBy, IEnumerable<PurchaseReturnLineRequest> lines, CancellationToken ct = default);
    Task<bool> ApproveAsync(string organizationId, string purchaseReturnId, CancellationToken ct = default);
    Task<bool> CloseAsync(string organizationId, string purchaseReturnId, CancellationToken ct = default);
}

public record PurchaseReturnLineRequest(string ProductId, decimal Quantity, decimal UnitCost);

