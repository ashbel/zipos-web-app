using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Infrastructure;
using POS.Shared.Domain;
using System.Linq;

namespace POS.Modules.Reporting.Services;

public class ProfitabilityReportService : IProfitabilityReportService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ProfitabilityReportService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<ProfitabilitySummaryDto> GetSummaryAsync(string organizationId, DateTime fromDate, DateTime toDate, string? branchId = null, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var q = _db.Set<Sale>().AsNoTracking().Where(s => s.TransactionDate >= fromDate && s.TransactionDate <= toDate);
        if (!string.IsNullOrWhiteSpace(branchId)) q = q.Where(s => s.BranchId == branchId);
        var revenue = await q.SumAsync(s => (decimal?)s.TotalAmount, ct) ?? 0m;
        var cogs = await q.SumAsync(s => (decimal?)s.CogsAmount, ct) ?? 0m;
        var gp = Math.Round(revenue - cogs, 2);
        var margin = revenue > 0 ? Math.Round(gp / revenue * 100m, 2) : 0m;
        return new ProfitabilitySummaryDto(revenue, cogs, gp, margin);
    }

    public async Task<IReadOnlyList<ProductProfitDto>> GetProductProfitAsync(string organizationId, DateTime fromDate, DateTime toDate, string? branchId = null, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var q = from si in _db.Set<SaleItem>().AsNoTracking()
                join s in _db.Set<Sale>().AsNoTracking() on si.SaleId equals s.Id
                where s.TransactionDate >= fromDate && s.TransactionDate <= toDate
                select new { s.BranchId, si.ProductId, si.Name, si.TotalAmount, si.CogsAmount };
        if (!string.IsNullOrWhiteSpace(branchId)) q = q.Where(x => x.BranchId == branchId);
        var grouped = await q.GroupBy(x => new { x.ProductId, x.Name })
            .Select(g => new ProductProfitDto(
                g.Key.ProductId,
                g.Key.Name,
                g.Sum(x => x.TotalAmount),
                g.Sum(x => x.CogsAmount),
                g.Sum(x => x.TotalAmount) - g.Sum(x => x.CogsAmount),
                g.Sum(x => x.TotalAmount) > 0 ? Math.Round((g.Sum(x => x.TotalAmount) - g.Sum(x => x.CogsAmount)) / g.Sum(x => x.TotalAmount) * 100m, 2) : 0m))
            .OrderByDescending(x => x.GrossProfit)
            .ToListAsync(ct);
        return grouped;
    }
}

