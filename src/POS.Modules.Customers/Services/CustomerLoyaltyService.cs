using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Customers.Services;

public class CustomerLoyaltyService : ICustomerLoyaltyService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public CustomerLoyaltyService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<CustomerLoyalty> GetAsync(string organizationId, string customerId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var cl = await _db.Set<CustomerLoyalty>().FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
        if (cl == null)
        {
            cl = new CustomerLoyalty { CustomerId = customerId };
            _db.Set<CustomerLoyalty>().Add(cl);
            await _db.SaveChangesAsync(ct);
        }
        return cl;
    }

    public async Task<CustomerLoyalty> AddPointsAsync(string organizationId, string customerId, int points, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var cl = await GetAsync(organizationId, customerId, ct);
        cl.Points += points;
        cl.LastUpdated = DateTime.UtcNow;
        // TODO: update tier based on thresholds
        await _db.SaveChangesAsync(ct);
        return cl;
    }
}

