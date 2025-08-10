using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Inventory.Services;

public class StocktakeService : IStocktakeService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public StocktakeService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<StocktakeSession> StartSessionAsync(string organizationId, string branchId, string startedBy, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var session = new StocktakeSession { BranchId = branchId, StartedBy = startedBy, Status = "Open", StartedAt = DateTime.UtcNow };
        _db.Set<StocktakeSession>().Add(session);
        await _db.SaveChangesAsync(ct);
        return session;
    }

    public async Task<StocktakeLine> UpsertCountAsync(string organizationId, string sessionId, string productId, decimal countedQty, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var session = await _db.Set<StocktakeSession>().FirstOrDefaultAsync(s => s.Id == sessionId && s.Status == "Open", ct);
        if (session == null) throw new InvalidOperationException("Session not open or not found");

        var expected = await _db.Set<InventoryItem>().AsNoTracking().FirstOrDefaultAsync(i => i.ProductId == productId && i.BranchId == session.BranchId, ct);
        var expectedQty = expected?.CurrentStock ?? 0m;

        var line = await _db.Set<StocktakeLine>().FirstOrDefaultAsync(l => l.SessionId == sessionId && l.ProductId == productId, ct);
        if (line == null)
        {
            line = new StocktakeLine { SessionId = sessionId, ProductId = productId, ExpectedQty = expectedQty };
            _db.Set<StocktakeLine>().Add(line);
        }
        line.CountedQty = countedQty;
        line.VarianceQty = countedQty - expectedQty;
        await _db.SaveChangesAsync(ct);
        return line;
    }

    public async Task<StocktakeSession> FinalizeSessionAsync(string organizationId, string sessionId, string finalizedBy, bool createAdjustments, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var session = await _db.Set<StocktakeSession>().FirstOrDefaultAsync(s => s.Id == sessionId && s.Status == "Open", ct) ?? throw new InvalidOperationException("Session not open or not found");
        session.Status = "Finalized";
        session.FinalizedBy = finalizedBy;
        session.FinalizedAt = DateTime.UtcNow;

        if (createAdjustments)
        {
            var lines = await _db.Set<StocktakeLine>().AsNoTracking().Where(l => l.SessionId == sessionId && l.VarianceQty != 0).ToListAsync(ct);
            foreach (var line in lines)
            {
                // Create stock adjustment per variance
                var adj = new StockAdjustment
                {
                    ProductId = line.ProductId,
                    BranchId = session.BranchId,
                    QuantityDelta = line.VarianceQty,
                    Reason = "Stocktake variance",
                    RequestedBy = finalizedBy,
                    Status = "Approved",
                    RequestedAt = DateTime.UtcNow,
                    ApprovedBy = finalizedBy,
                    ApprovedAt = DateTime.UtcNow
                };
                _db.Set<StockAdjustment>().Add(adj);
                // Apply to inventory
                var inv = await _db.Set<InventoryItem>().FirstOrDefaultAsync(i => i.ProductId == line.ProductId && i.BranchId == session.BranchId, ct);
                if (inv == null)
                {
                    inv = new InventoryItem { ProductId = line.ProductId, BranchId = session.BranchId };
                    _db.Set<InventoryItem>().Add(inv);
                }
                inv.CurrentStock += line.VarianceQty;
                inv.LastUpdated = DateTime.UtcNow;
                _db.Set<StockMovement>().Add(new StockMovement { ProductId = line.ProductId, BranchId = session.BranchId, QuantityDelta = line.VarianceQty, Reason = "Stocktake variance", PerformedAt = DateTime.UtcNow });
            }
        }

        await _db.SaveChangesAsync(ct);
        return session;
    }

    public async Task<IReadOnlyList<StocktakeLine>> GetSessionLinesAsync(string organizationId, string sessionId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        return await _db.Set<StocktakeLine>().AsNoTracking().Where(l => l.SessionId == sessionId).ToListAsync(ct);
    }
}

