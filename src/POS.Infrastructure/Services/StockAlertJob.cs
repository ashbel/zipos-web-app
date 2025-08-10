using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using POS.Infrastructure.Data;
using POS.Shared.Infrastructure;
using POS.Shared.Domain;

namespace POS.Infrastructure.Services;

public class StockAlertJob
{
    private readonly IServiceProvider _services;
    private readonly ITenantMetadataStore _tenantMetadataStore;
    private readonly ITenantContext _tenantContext;

    public StockAlertJob(IServiceProvider services, ITenantMetadataStore tenantMetadataStore, ITenantContext tenantContext)
    {
        _services = services;
        _tenantMetadataStore = tenantMetadataStore;
        _tenantContext = tenantContext;
    }

    public async Task RunAsync(string organizationId, CancellationToken ct = default)
    {
        using var scope = _services.CreateScope();
        _tenantContext.SetTenant(organizationId);
        var db = scope.ServiceProvider.GetRequiredService<POSDbContext>();

        var lows = await db.Set<InventoryItem>()
            .AsNoTracking()
            .Where(i => i.ReorderLevel > 0 && i.CurrentStock <= i.ReorderLevel)
            .Select(i => new { i.ProductId, i.BranchId, i.CurrentStock, i.ReorderLevel })
            .ToListAsync(ct);

        foreach (var lo in lows)
        {
            var alert = await db.Set<StockAlert>().FirstOrDefaultAsync(a => a.ProductId == lo.ProductId && a.BranchId == lo.BranchId, ct);
            if (alert == null)
            {
                alert = new StockAlert
                {
                    ProductId = lo.ProductId,
                    BranchId = lo.BranchId,
                    CurrentStock = lo.CurrentStock,
                    ReorderLevel = lo.ReorderLevel,
                    IsAcknowledged = false
                };
                db.Set<StockAlert>().Add(alert);
            }
            else
            {
                alert.CurrentStock = lo.CurrentStock;
                alert.ReorderLevel = lo.ReorderLevel;
                alert.IsAcknowledged = false;
            }
        }

        // Clear alerts where stock recovered
        var toClear = await db.Set<StockAlert>()
            .Where(a => !a.IsAcknowledged)
            .ToListAsync(ct);
        foreach (var alert in toClear)
        {
            var inv = await db.Set<InventoryItem>().AsNoTracking().FirstOrDefaultAsync(i => i.ProductId == alert.ProductId && i.BranchId == alert.BranchId, ct);
            if (inv != null && inv.CurrentStock > inv.ReorderLevel)
            {
                db.Set<StockAlert>().Remove(alert);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}

