using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Customers.Services;

public class CustomerService : ICustomerService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public CustomerService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Customer> CreateAsync(string organizationId, CreateCustomerRequest request, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email ?? string.Empty,
            Phone = request.Phone ?? string.Empty,
            TaxId = request.TaxId ?? string.Empty,
            Notes = request.Notes ?? string.Empty,
            IsActive = true
        };
        _db.Set<Customer>().Add(customer);
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    public async Task<Customer?> UpdateAsync(string organizationId, string customerId, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var customer = await _db.Set<Customer>().FirstOrDefaultAsync(c => c.Id == customerId, ct);
        if (customer == null) return null;
        if (!string.IsNullOrWhiteSpace(request.Name)) customer.Name = request.Name!;
        if (!string.IsNullOrWhiteSpace(request.Email)) customer.Email = request.Email!;
        if (!string.IsNullOrWhiteSpace(request.Phone)) customer.Phone = request.Phone!;
        if (!string.IsNullOrWhiteSpace(request.TaxId)) customer.TaxId = request.TaxId!;
        if (!string.IsNullOrWhiteSpace(request.Notes)) customer.Notes = request.Notes!;
        if (!string.IsNullOrWhiteSpace(request.LoyaltyTier)) customer.LoyaltyTier = request.LoyaltyTier!;
        if (request.IsActive.HasValue) customer.IsActive = request.IsActive.Value;
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    public async Task<bool> DeleteAsync(string organizationId, string customerId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var customer = await _db.Set<Customer>().FirstOrDefaultAsync(c => c.Id == customerId, ct);
        if (customer == null) return false;
        _db.Set<Customer>().Remove(customer);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(string organizationId, string? query, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var q = _db.Set<Customer>().AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            q = q.Where(c => c.Name.Contains(query) || c.Email.Contains(query) || c.Phone.Contains(query));
        }
        return await q.OrderBy(c => c.Name).Take(100).ToListAsync(ct);
    }
}