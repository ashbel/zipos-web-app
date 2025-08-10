using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Infrastructure;
using POS.Shared.Domain;
using System.Linq;

namespace POS.Modules.Customers.Services;

public class CustomerAnalyticsService : ICustomerAnalyticsService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public CustomerAnalyticsService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<CustomerAnalyticsDto> GetAnalyticsAsync(string organizationId, string customerId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var query = _db.Set<Sale>().AsNoTracking().Where(s => s.CustomerId == customerId);
        var totalSpent = await query.SumAsync(s => (decimal?)s.TotalAmount, ct) ?? 0m;
        var count = await query.CountAsync(ct);
        var average = count > 0 ? Math.Round(totalSpent / count, 2) : 0m;
        var last = await query.OrderByDescending(s => s.TransactionDate).Select(s => (DateTime?)s.TransactionDate).FirstOrDefaultAsync(ct);
        var days = last.HasValue ? (int)Math.Floor((DateTime.UtcNow - last.Value).TotalDays) : int.MaxValue;
        return new CustomerAnalyticsDto(totalSpent, count, average, last, days);
    }
}

