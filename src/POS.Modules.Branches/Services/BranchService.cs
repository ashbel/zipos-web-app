using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Branches.Services;

public class BranchService : IBranchService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public BranchService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Branch> CreateBranchAsync(string organizationId, CreateBranchRequest request, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var branch = new Branch
        {
            Name = request.Name,
            Code = request.Code,
            ContactPhone = request.ContactPhone ?? string.Empty,
            ContactEmail = request.ContactEmail ?? string.Empty,
            TimeZone = request.TimeZone,
            Currency = request.Currency,
            TaxRate = request.TaxRate,
            TaxInclusive = request.TaxInclusive,
            IsActive = true
        };
        _db.Branches.Add(branch);
        await _db.SaveChangesAsync(ct);
        return branch;
    }

    public async Task<Branch?> UpdateBranchAsync(string organizationId, string branchId, UpdateBranchRequest request, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == branchId, ct);
        if (branch == null) return null;
        if (!string.IsNullOrWhiteSpace(request.Name)) branch.Name = request.Name!;
        if (!string.IsNullOrWhiteSpace(request.ContactPhone)) branch.ContactPhone = request.ContactPhone!;
        if (!string.IsNullOrWhiteSpace(request.ContactEmail)) branch.ContactEmail = request.ContactEmail!;
        if (!string.IsNullOrWhiteSpace(request.TimeZone)) branch.TimeZone = request.TimeZone!;
        if (!string.IsNullOrWhiteSpace(request.Currency)) branch.Currency = request.Currency!;
        if (request.TaxRate.HasValue) branch.TaxRate = request.TaxRate.Value;
        if (request.TaxInclusive.HasValue) branch.TaxInclusive = request.TaxInclusive.Value;
        if (request.IsActive.HasValue) branch.IsActive = request.IsActive.Value;
        await _db.SaveChangesAsync(ct);
        return branch;
    }

    public async Task<bool> DeleteBranchAsync(string organizationId, string branchId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var branch = await _db.Branches.FirstOrDefaultAsync(b => b.Id == branchId, ct);
        if (branch == null) return false;
        _db.Branches.Remove(branch);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<Branch>> GetBranchesAsync(string organizationId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        return await _db.Branches.AsNoTracking().ToListAsync(ct);
    }
}

