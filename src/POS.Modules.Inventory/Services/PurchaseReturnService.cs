using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Inventory.Services;

public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public PurchaseReturnService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<PurchaseReturn> CreateAsync(string organizationId, string supplierId, string branchId, string reason, string createdBy, IEnumerable<PurchaseReturnLineRequest> lines, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var pr = new PurchaseReturn { SupplierId = supplierId, BranchId = branchId, Reason = reason, Status = "Submitted", CreatedBy = createdBy, CreatedAt = DateTime.UtcNow };
        _db.Set<PurchaseReturn>().Add(pr);
        await _db.SaveChangesAsync(ct);
        foreach (var l in lines)
        {
            _db.Set<PurchaseReturnLine>().Add(new PurchaseReturnLine { PurchaseReturnId = pr.Id, ProductId = l.ProductId, Quantity = l.Quantity, UnitCost = l.UnitCost });
        }
        await _db.SaveChangesAsync(ct);
        return pr;
    }

    public async Task<bool> ApproveAsync(string organizationId, string purchaseReturnId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var pr = await _db.Set<PurchaseReturn>().FirstOrDefaultAsync(x => x.Id == purchaseReturnId, ct);
        if (pr == null || pr.Status != "Submitted") return false;
        pr.Status = "Approved";
        await _db.SaveChangesAsync(ct);

        // Deduct stock back to supplier and adjust average cost (keep average stable; reduce quantity)
        var lines = await _db.Set<PurchaseReturnLine>().Where(l => l.PurchaseReturnId == purchaseReturnId).ToListAsync(ct);
        foreach (var l in lines)
        {
            var inv = await _db.Set<InventoryItem>().FirstOrDefaultAsync(i => i.ProductId == l.ProductId && i.BranchId == pr.BranchId, ct);
            if (inv == null) continue;
            inv.CurrentStock = Math.Max(0, inv.CurrentStock - l.Quantity);
            inv.LastUpdated = DateTime.UtcNow;
            _db.Set<StockMovement>().Add(new StockMovement { ProductId = l.ProductId, BranchId = pr.BranchId, QuantityDelta = -l.Quantity, Reason = "Purchase return" });
        }
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CloseAsync(string organizationId, string purchaseReturnId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var pr = await _db.Set<PurchaseReturn>().FirstOrDefaultAsync(x => x.Id == purchaseReturnId, ct);
        if (pr == null || pr.Status != "Approved") return false;
        pr.Status = "Closed";
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

