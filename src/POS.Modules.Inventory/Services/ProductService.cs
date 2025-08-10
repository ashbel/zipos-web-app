using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;

namespace POS.Modules.Inventory.Services;

public class ProductService : IProductService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;

    public ProductService(POSDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<Product> CreateProductAsync(string organizationId, CreateProductRequest request, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var product = new Product
        {
            SKU = request.SKU,
            Barcode = request.Barcode,
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            BasePrice = request.BasePrice,
            Cost = request.Cost,
            IsActive = true
        };
        _db.Set<Product>().Add(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<Product?> UpdateProductAsync(string organizationId, string productId, UpdateProductRequest request, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var product = await _db.Set<Product>().FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product == null) return null;
        if (!string.IsNullOrWhiteSpace(request.SKU)) product.SKU = request.SKU!;
        if (!string.IsNullOrWhiteSpace(request.Barcode)) product.Barcode = request.Barcode!;
        if (!string.IsNullOrWhiteSpace(request.Name)) product.Name = request.Name!;
        if (!string.IsNullOrWhiteSpace(request.Description)) product.Description = request.Description!;
        if (request.BasePrice.HasValue) product.BasePrice = request.BasePrice.Value;
        if (request.Cost.HasValue) product.Cost = request.Cost.Value;
        if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<bool> DeleteProductAsync(string organizationId, string productId, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var product = await _db.Set<Product>().FirstOrDefaultAsync(p => p.Id == productId, ct);
        if (product == null) return false;
        _db.Set<Product>().Remove(product);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<Product>> SearchProductsAsync(string organizationId, string? query, CancellationToken ct = default)
    {
        _tenantContext.SetTenant(organizationId);
        var q = _db.Set<Product>().AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            q = q.Where(p => p.Name.Contains(query) || p.SKU.Contains(query) || p.Barcode.Contains(query));
        }
        return await q.OrderBy(p => p.Name).Take(100).ToListAsync(ct);
    }
}

