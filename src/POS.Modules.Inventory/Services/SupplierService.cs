using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Inventory.Services;

public class SupplierService : ISupplierService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public SupplierService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Supplier> CreateSupplierAsync(string organizationId, CreateSupplierRequest request, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var s = new Supplier
        {
            Name = request.Name,
            Email = request.Email ?? string.Empty,
            Phone = request.Phone ?? string.Empty,
            PaymentTerms = request.PaymentTerms ?? string.Empty,
            Address = request.Address ?? string.Empty,
            IsActive = true
        };
        _db.Set<Supplier>().Add(s);
        await _db.SaveChangesAsync(ct);
        return s;
    }

    public async Task<Supplier?> UpdateSupplierAsync(string organizationId, string supplierId, UpdateSupplierRequest request, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var s = await _db.Set<Supplier>().FirstOrDefaultAsync(x => x.Id == supplierId, ct);
        if (s == null) return null;
        if (!string.IsNullOrWhiteSpace(request.Name)) s.Name = request.Name!;
        if (!string.IsNullOrWhiteSpace(request.Email)) s.Email = request.Email!;
        if (!string.IsNullOrWhiteSpace(request.Phone)) s.Phone = request.Phone!;
        if (!string.IsNullOrWhiteSpace(request.PaymentTerms)) s.PaymentTerms = request.PaymentTerms!;
        if (!string.IsNullOrWhiteSpace(request.Address)) s.Address = request.Address!;
        if (request.IsActive.HasValue) s.IsActive = request.IsActive.Value;
        await _db.SaveChangesAsync(ct);
        return s;
    }

    public async Task<bool> DeleteSupplierAsync(string organizationId, string supplierId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var s = await _db.Set<Supplier>().FirstOrDefaultAsync(x => x.Id == supplierId, ct);
        if (s == null) return false;
        _db.Set<Supplier>().Remove(s);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<Supplier>> SearchSuppliersAsync(string organizationId, string? q, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var query = _db.Set<Supplier>().AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            query = query.Where(s => s.Name.Contains(q) || s.Email.Contains(q) || s.Phone.Contains(q));
        }
        return await query.OrderBy(s => s.Name).Take(100).ToListAsync(ct);
    }
}

