using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Customers.Services;

public class CustomerHistoryService : ICustomerHistoryService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public CustomerHistoryService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<Sale>> GetPurchaseHistoryAsync(string organizationId, string customerId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        return await _db.Set<Sale>().AsNoTracking().Where(s => s.CustomerId == customerId).OrderByDescending(s => s.TransactionDate).Take(200).ToListAsync(ct);
    }
}

