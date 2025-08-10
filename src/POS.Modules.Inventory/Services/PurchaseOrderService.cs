using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Inventory.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public PurchaseOrderService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<PurchaseOrder> CreateAsync(string organizationId, string supplierId, string branchId, string createdBy, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var po = new PurchaseOrder { SupplierId = supplierId, BranchId = branchId, Status = "Draft", CreatedBy = createdBy, CreatedAt = DateTime.UtcNow };
        _db.Set<PurchaseOrder>().Add(po);
        await _db.SaveChangesAsync(ct);
        return po;
    }

    public async Task<PurchaseOrderLine> AddLineAsync(string organizationId, string purchaseOrderId, string productId, decimal quantityOrdered, decimal unitCost, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var po = await _db.Set<PurchaseOrder>().FirstOrDefaultAsync(x => x.Id == purchaseOrderId, ct) ?? throw new InvalidOperationException("PO not found");
        if (po.Status != "Draft" && po.Status != "Submitted") throw new InvalidOperationException("Cannot modify lines for this status");
        var line = new PurchaseOrderLine { PurchaseOrderId = purchaseOrderId, ProductId = productId, QuantityOrdered = quantityOrdered, UnitCost = unitCost, QuantityReceived = 0 };
        _db.Set<PurchaseOrderLine>().Add(line);
        await _db.SaveChangesAsync(ct);
        return line;
    }

    public async Task<bool> SubmitAsync(string organizationId, string purchaseOrderId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var po = await _db.Set<PurchaseOrder>().FirstOrDefaultAsync(x => x.Id == purchaseOrderId, ct);
        if (po == null || po.Status != "Draft") return false;
        po.Status = "Submitted";
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ApproveAsync(string organizationId, string purchaseOrderId, string approvedBy, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var po = await _db.Set<PurchaseOrder>().FirstOrDefaultAsync(x => x.Id == purchaseOrderId, ct);
        if (po == null || po.Status != "Submitted") return false;
        po.Status = "Approved";
        po.ApprovedBy = approvedBy;
        po.ApprovedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<GoodsReceipt> ReceiveAsync(string organizationId, string purchaseOrderId, string receivedBy, IEnumerable<ReceiveLineRequest> lines, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var po = await _db.Set<PurchaseOrder>().FirstOrDefaultAsync(x => x.Id == purchaseOrderId, ct) ?? throw new InvalidOperationException("PO not found");
        if (po.Status != "Approved" && po.Status != "Receiving") throw new InvalidOperationException("PO not approved");
        po.Status = "Receiving";

        var receipt = new GoodsReceipt { PurchaseOrderId = purchaseOrderId, ReceivedBy = receivedBy, ReceivedAt = DateTime.UtcNow };
        _db.Set<GoodsReceipt>().Add(receipt);
        await _db.SaveChangesAsync(ct);

        var lineMap = await _db.Set<PurchaseOrderLine>().Where(l => l.PurchaseOrderId == purchaseOrderId).ToDictionaryAsync(l => l.Id, ct);
        foreach (var rl in lines)
        {
            if (!lineMap.TryGetValue(rl.PurchaseOrderLineId, out var pol)) continue;
            var toApply = Math.Min(rl.QuantityReceived, pol.QuantityOrdered - pol.QuantityReceived);
            pol.QuantityReceived += toApply;
            _db.Set<GoodsReceiptLine>().Add(new GoodsReceiptLine { GoodsReceiptId = receipt.Id, PurchaseOrderLineId = pol.Id, ProductId = rl.ProductId, QuantityReceived = toApply, UnitCost = rl.UnitCost });

            // Update inventory average cost (simplified) and stock
            var inv = await _db.Set<InventoryItem>().FirstOrDefaultAsync(i => i.ProductId == rl.ProductId && i.BranchId == po.BranchId, ct);
            if (inv == null)
            {
                inv = new InventoryItem { ProductId = rl.ProductId, BranchId = po.BranchId, CurrentStock = 0, AverageCost = rl.UnitCost, LastPurchasePrice = rl.UnitCost };
                _db.Set<InventoryItem>().Add(inv);
            }
            else
            {
                var totalExistingCost = inv.AverageCost * inv.CurrentStock;
                var totalNewCost = rl.UnitCost * toApply;
                var newQty = inv.CurrentStock + toApply;
                inv.AverageCost = newQty > 0 ? Math.Round((totalExistingCost + totalNewCost) / newQty, 4) : inv.AverageCost;
                inv.LastPurchasePrice = rl.UnitCost;
            }
            inv.CurrentStock += toApply;
            inv.LastUpdated = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        return receipt;
    }

    public async Task<bool> CloseAsync(string organizationId, string purchaseOrderId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var po = await _db.Set<PurchaseOrder>().FirstOrDefaultAsync(x => x.Id == purchaseOrderId, ct);
        if (po == null) return false;
        var allReceived = await _db.Set<PurchaseOrderLine>().Where(l => l.PurchaseOrderId == purchaseOrderId).AllAsync(l => l.QuantityReceived >= l.QuantityOrdered, ct);
        if (!allReceived) return false;
        po.Status = "Closed";
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

