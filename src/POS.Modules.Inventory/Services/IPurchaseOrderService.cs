using POS.Shared.Domain;

namespace POS.Modules.Inventory.Services;

public interface IPurchaseOrderService
{
    Task<PurchaseOrder> CreateAsync(string organizationId, string supplierId, string branchId, string createdBy, CancellationToken ct = default);
    Task<PurchaseOrderLine> AddLineAsync(string organizationId, string purchaseOrderId, string productId, decimal quantityOrdered, decimal unitCost, CancellationToken ct = default);
    Task<bool> SubmitAsync(string organizationId, string purchaseOrderId, CancellationToken ct = default);
    Task<bool> ApproveAsync(string organizationId, string purchaseOrderId, string approvedBy, CancellationToken ct = default);
    Task<GoodsReceipt> ReceiveAsync(string organizationId, string purchaseOrderId, string receivedBy, IEnumerable<ReceiveLineRequest> lines, CancellationToken ct = default);
    Task<bool> CloseAsync(string organizationId, string purchaseOrderId, CancellationToken ct = default);
}

public record ReceiveLineRequest(string PurchaseOrderLineId, string ProductId, decimal QuantityReceived, decimal UnitCost);

