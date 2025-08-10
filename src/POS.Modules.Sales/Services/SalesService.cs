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

    public async Task<Sale> CheckoutAsync(string organizationId, string cartId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var cart = await _db.Set<Cart>().AsNoTracking().FirstAsync(c => c.Id == cartId, ct);
        var items = await _db.Set<CartItem>().AsNoTracking().Where(i => i.CartId == cartId).ToListAsync(ct);

        var sale = new Sale
        {
            BranchId = cart.BranchId,
            UserId = cart.UserId,
            SubTotal = items.Sum(i => i.UnitPrice * i.Quantity),
            DiscountAmount = items.Sum(i => i.DiscountAmount),
            TaxAmount = 0, // TBD: tax calculation
            TotalAmount = items.Sum(i => i.TotalAmount),
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

        // Clear cart
        _db.Set<Cart>().Remove(await _db.Set<Cart>().FirstAsync(c => c.Id == cartId, ct));
        _db.Set<CartItem>().RemoveRange(_db.Set<CartItem>().Where(ci => ci.CartId == cartId));
        await _db.SaveChangesAsync(ct);

        return sale;
    }
}

