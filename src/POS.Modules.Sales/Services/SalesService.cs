using Microsoft.EntityFrameworkCore;
using POS.Modules.Inventory.Services;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Sales.Services;

public class SalesService : ISalesService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IInventoryService _inventoryService;

    public SalesService(POSDbContext db, ITenantContext tenantContext, IInventoryService inventoryService)
    {
        _db = db;
        _tenantContext = tenantContext;
        _inventoryService = inventoryService;
    }

    public async Task<Cart> CreateCartAsync(string organizationId, string userId, string branchId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var cart = new Cart { UserId = userId, BranchId = branchId, TotalAmount = 0 };
        _db.Set<Cart>().Add(cart);
        await _db.SaveChangesAsync(ct);
        return cart;
    }

    public async Task<Cart> AddItemAsync(string organizationId, string cartId, string productId, string name, decimal quantity, decimal unitPrice, decimal discount, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var cart = await _db.Set<Cart>().FirstAsync(c => c.Id == cartId, ct);
        var item = new CartItem
        {
            CartId = cartId,
            ProductId = productId,
            Name = name,
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountAmount = discount,
            TotalAmount = (unitPrice * quantity) - discount
        };
        _db.Set<CartItem>().Add(item);
        cart.TotalAmount += item.TotalAmount;
        await _db.SaveChangesAsync(ct);
        return cart;
    }

    public async Task<Cart> RemoveItemAsync(string organizationId, string cartId, string itemId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var cart = await _db.Set<Cart>().FirstAsync(c => c.Id == cartId, ct);
        var item = await _db.Set<CartItem>().FirstAsync(ci => ci.Id == itemId && ci.CartId == cartId, ct);
        cart.TotalAmount -= item.TotalAmount;
        _db.Set<CartItem>().Remove(item);
        await _db.SaveChangesAsync(ct);
        return cart;
    }

    public async Task<Sale> CheckoutAsync(string organizationId, string cartId, IEnumerable<PaymentRequest> payments, string? customerId = null, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var cart = await _db.Set<Cart>().AsNoTracking().FirstAsync(c => c.Id == cartId, ct);
        var items = await _db.Set<CartItem>().AsNoTracking().Where(i => i.CartId == cartId).ToListAsync(ct);

        var sale = new Sale
        {
            BranchId = cart.BranchId,
            CustomerId = customerId ?? string.Empty,
            UserId = cart.UserId,
            SubTotal = items.Sum(i => i.UnitPrice * i.Quantity),
            DiscountAmount = items.Sum(i => i.DiscountAmount),
            TaxAmount = CalculateTax(cart.BranchId, items),
            TotalAmount = items.Sum(i => i.TotalAmount) + CalculateTax(cart.BranchId, items),
            Status = "Completed",
            TransactionDate = DateTime.UtcNow
        };
        _db.Set<Sale>().Add(sale);
        await _db.SaveChangesAsync(ct);

        foreach (var i in items)
        {
            var saleItem = new SaleItem
            {
                SaleId = sale.Id,
                ProductId = i.ProductId,
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                DiscountAmount = i.DiscountAmount,
                TotalAmount = i.TotalAmount
            };
            _db.Set<SaleItem>().Add(saleItem);

            // Deduct inventory
            await _inventoryService.AdjustStockAsync(organizationId, new Modules.Inventory.Services.AdjustStockRequest(i.ProductId, cart.BranchId, -i.Quantity, "Sale checkout"), ct);
        }

        // Record payments (split supported)
        var paymentsList = payments?.ToList() ?? new List<PaymentRequest>();
        var paid = paymentsList.Sum(p => p.Amount);
        if (paid != sale.TotalAmount)
        {
            throw new InvalidOperationException("Payment total must equal sale total");
        }
        foreach (var p in paymentsList)
        {
            _db.Set<Payment>().Add(new Payment { SaleId = sale.Id, Method = p.Method, Amount = p.Amount, Reference = p.Reference, Status = "Captured" });
        }

        // Loyalty accrual
        if (!string.IsNullOrWhiteSpace(customerId))
        {
            try
            {
                var loyalty = _db.GetService<POS.Modules.Customers.Services.ICustomerLoyaltyService>();
                if (loyalty != null)
                {
                    // Example: 1 point per currency unit
                    var points = (int)Math.Floor(sale.TotalAmount);
                    await loyalty.AddPointsAsync(organizationId, customerId!, points, ct);
                }
            }
            catch
            {
                // swallow loyalty accrual failures to not block checkout
            }
        }

        // Clear cart
        _db.Set<Cart>().Remove(await _db.Set<Cart>().FirstAsync(c => c.Id == cartId, ct));
        _db.Set<CartItem>().RemoveRange(_db.Set<CartItem>().Where(ci => ci.CartId == cartId));
        await _db.SaveChangesAsync(ct);

        return sale;
    }

    private static decimal CalculateTax(string branchId, List<CartItem> items)
    {
        // Placeholder: flat 0% tax. Extend to read branch tax settings and compute
        return 0m;
    }

    public async Task<Refund> ProcessRefundAsync(string organizationId, string saleId, IEnumerable<RefundLineRequest> items, string reason, string processedBy, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var sale = await _db.Set<Sale>().FirstOrDefaultAsync(s => s.Id == saleId, ct) ?? throw new InvalidOperationException("Sale not found");
        var saleItems = await _db.Set<SaleItem>().Where(si => si.SaleId == saleId).ToListAsync(ct);

        var refund = new Refund { SaleId = saleId, Amount = 0, Reason = reason, ProcessedBy = processedBy, ProcessedAt = DateTime.UtcNow, Status = "Pending" };
        _db.Set<Refund>().Add(refund);
        await _db.SaveChangesAsync(ct);

        decimal totalRefund = 0m;
        foreach (var line in items)
        {
            var si = saleItems.FirstOrDefault(x => x.Id == line.SaleItemId && x.ProductId == line.ProductId) ?? throw new InvalidOperationException("Sale item not found");
            if (line.Quantity <= 0 || line.Quantity > si.Quantity) throw new InvalidOperationException("Invalid refund quantity");

            var amount = Math.Round((si.UnitPrice * line.Quantity) - (si.DiscountAmount * (line.Quantity / si.Quantity)), 2);
            totalRefund += amount;
            var ri = new RefundItem
            {
                RefundId = refund.Id,
                SaleItemId = si.Id,
                ProductId = si.ProductId,
                Quantity = line.Quantity,
                UnitPrice = si.UnitPrice,
                DiscountAmount = Math.Round(si.DiscountAmount * (line.Quantity / si.Quantity), 2),
                TotalAmount = amount,
                Restocked = line.Restock
            };
            _db.Set<RefundItem>().Add(ri);

            // Restock only after approval
        }

        refund.Amount = totalRefund;
        await _db.SaveChangesAsync(ct);

        // Mark sale as refunded if fully refunded
        // Do not mark sale refunded until approval

        return refund;
    }

    public async Task<bool> ApproveRefundAsync(string organizationId, string refundId, string approvedBy, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var refund = await _db.Set<Refund>().FirstOrDefaultAsync(r => r.Id == refundId, ct);
        if (refund == null || refund.Status != "Pending") return false;
        var sale = await _db.Set<Sale>().FirstAsync(s => s.Id == refund.SaleId, ct);
        var lines = await _db.Set<RefundItem>().Where(ri => ri.RefundId == refundId).ToListAsync(ct);

        foreach (var line in lines)
        {
            if (line.Restocked)
            {
                await _inventoryService.AdjustStockAsync(organizationId, new Modules.Inventory.Services.AdjustStockRequest(line.ProductId, sale.BranchId, line.Quantity, "Refund restock (approved)"), ct);
            }
        }

        refund.Status = "Approved";
        await _db.SaveChangesAsync(ct);

        var totalApprovedRefunds = await _db.Set<Refund>().Where(r => r.SaleId == sale.Id && r.Status == "Approved").SumAsync(r => r.Amount, ct);
        if (totalApprovedRefunds >= sale.TotalAmount)
        {
            sale.Status = "Refunded";
            await _db.SaveChangesAsync(ct);
        }
        return true;
    }

    public async Task<bool> RejectRefundAsync(string organizationId, string refundId, string rejectedBy, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var refund = await _db.Set<Refund>().FirstOrDefaultAsync(r => r.Id == refundId, ct);
        if (refund == null || refund.Status != "Pending") return false;
        refund.Status = "Rejected";
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

