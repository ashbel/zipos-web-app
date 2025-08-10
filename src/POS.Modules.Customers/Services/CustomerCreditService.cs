using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Customers.Services;

public class CustomerCreditService : ICustomerCreditService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public CustomerCreditService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<CustomerCredit> GetAsync(string organizationId, string customerId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var credit = await _db.Set<CustomerCredit>().FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);
        if (credit == null)
        {
            credit = new CustomerCredit { CustomerId = customerId, CreditLimit = 0, OutstandingBalance = 0, Status = "Active" };
            _db.Set<CustomerCredit>().Add(credit);
            await _db.SaveChangesAsync(ct);
        }
        return credit;
    }

    public async Task<CustomerCredit> SetLimitAsync(string organizationId, string customerId, decimal creditLimit, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var credit = await GetAsync(organizationId, customerId, ct);
        credit.CreditLimit = creditLimit;
        credit.LastUpdated = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return credit;
    }

    public async Task<bool> RecordPaymentAsync(string organizationId, string customerId, decimal amount, string? reference, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        if (amount <= 0) return false;
        var credit = await GetAsync(organizationId, customerId, ct);
        credit.OutstandingBalance = Math.Max(0, credit.OutstandingBalance - amount);
        credit.LastUpdated = DateTime.UtcNow;
        _db.Set<CustomerCreditTransaction>().Add(new CustomerCreditTransaction { CustomerId = customerId, Type = "Payment", Amount = amount, Reference = reference });
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ChargeAsync(string organizationId, string customerId, decimal amount, string? saleId, string? note, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        if (amount <= 0) return false;
        var credit = await GetAsync(organizationId, customerId, ct);
        if (credit.Status != "Active") return false;
        if (credit.OutstandingBalance + amount > credit.CreditLimit) return false;
        credit.OutstandingBalance += amount;
        credit.LastUpdated = DateTime.UtcNow;
        _db.Set<CustomerCreditTransaction>().Add(new CustomerCreditTransaction { CustomerId = customerId, Type = "Charge", Amount = amount, SaleId = saleId, Note = note });
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<CustomerCreditTransaction>> GetTransactionsAsync(string organizationId, string customerId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        return await _db.Set<CustomerCreditTransaction>().AsNoTracking().Where(t => t.CustomerId == customerId).OrderByDescending(t => t.OccurredAt).ToListAsync(ct);
    }
}