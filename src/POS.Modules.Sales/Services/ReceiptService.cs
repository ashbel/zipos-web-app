using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;
using System.Text;

namespace POS.Modules.Sales.Services;

public class ReceiptService : IReceiptService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ReceiptService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<string> GenerateReceiptAsync(string organizationId, string saleId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var sale = await _db.Set<Sale>().AsNoTracking().FirstAsync(s => s.Id == saleId, ct);
        var items = await _db.Set<SaleItem>().AsNoTracking().Where(i => i.SaleId == saleId).ToListAsync(ct);
        var pays = await _db.Set<Payment>().AsNoTracking().Where(p => p.SaleId == saleId).ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine($"Receipt #{sale.Id}");
        sb.AppendLine($"Date: {sale.TransactionDate:u}");
        sb.AppendLine("Items:");
        foreach (var i in items)
        {
            sb.AppendLine($" - {i.Name} x{i.Quantity} @ {i.UnitPrice:C} = {i.TotalAmount:C}");
        }
        sb.AppendLine($"Subtotal: {sale.SubTotal:C}");
        sb.AppendLine($"Discount: {sale.DiscountAmount:C}");
        sb.AppendLine($"Tax: {sale.TaxAmount:C}");
        sb.AppendLine($"Total: {sale.TotalAmount:C}");
        sb.AppendLine("Payments:");
        foreach (var p in pays)
        {
            sb.AppendLine($" - {p.Method}: {p.Amount:C} {(string.IsNullOrEmpty(p.Reference) ? string.Empty : "(" + p.Reference + ")")}");
        }
        return sb.ToString();
    }
}

