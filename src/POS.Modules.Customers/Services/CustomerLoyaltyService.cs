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
        // Update tier based on thresholds
        var tiers = await _db.Set<LoyaltyTierDefinition>().AsNoTracking().OrderByDescending(t => t.Priority).ThenByDescending(t => t.MinPoints).ToListAsync(ct);
        var matched = tiers.FirstOrDefault(t => cl.Points >= t.MinPoints && (!t.MaxPoints.HasValue || cl.Points <= t.MaxPoints.Value));
        if (matched != null)
        {
            cl.Tier = matched.Name;
        }
        await _db.SaveChangesAsync(ct);
        return cl;
    }

    public async Task<IReadOnlyList<LoyaltyTierDefinition>> GetTiersAsync(string organizationId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        return await _db.Set<LoyaltyTierDefinition>().AsNoTracking().OrderByDescending(t => t.Priority).ThenBy(t => t.MinPoints).ToListAsync(ct);
    }

    public async Task<LoyaltyTierDefinition> CreateOrUpdateTierAsync(string organizationId, LoyaltyTierDefinition tier, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var existing = await _db.Set<LoyaltyTierDefinition>().FirstOrDefaultAsync(t => t.Id == tier.Id || t.Name == tier.Name, ct);
        if (existing == null)
        {
            _db.Set<LoyaltyTierDefinition>().Add(tier);
        }
        else
        {
            existing.MinPoints = tier.MinPoints;
            existing.MaxPoints = tier.MaxPoints;
            existing.DiscountPercent = tier.DiscountPercent;
            existing.Priority = tier.Priority;
        }
        await _db.SaveChangesAsync(ct);
        return existing ?? tier;
    }
}

