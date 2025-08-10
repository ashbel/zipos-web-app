using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Inventory.Services;

public class InventoryService : IInventoryService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public InventoryService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<InventoryItem> AdjustStockAsync(string organizationId, AdjustStockRequest request, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var item = await _db.Set<InventoryItem>().FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.BranchId == request.BranchId, ct);
        if (item == null)
        {
            item = new InventoryItem
            {
                ProductId = request.ProductId,
                BranchId = request.BranchId,
                CurrentStock = 0,
                ReorderLevel = 0,
                AverageCost = 0,
                LastPurchasePrice = 0,
                LastUpdated = DateTime.UtcNow
            };
            _db.Set<InventoryItem>().Add(item);
        }
        item.CurrentStock += request.QuantityDelta;
        item.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        // Log stock movement
        _db.Set<StockMovement>().Add(new StockMovement
        {
            ProductId = request.ProductId,
            BranchId = request.BranchId,
            QuantityDelta = request.QuantityDelta,
            Reason = request.Reason,
            PerformedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        return item;
    }

    public async Task<InventoryItem?> GetInventoryAsync(string organizationId, string productId, string branchId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        return await _db.Set<InventoryItem>().AsNoTracking().FirstOrDefaultAsync(i => i.ProductId == productId && i.BranchId == branchId, ct);
    }
}

