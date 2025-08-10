using POS.Shared.Domain;

namespace POS.Modules.Inventory.Services;

public interface IProductService
{
    Task<Product> CreateProductAsync(string organizationId, CreateProductRequest request, CancellationToken ct = default);
    Task<Product?> UpdateProductAsync(string organizationId, string productId, UpdateProductRequest request, CancellationToken ct = default);
    Task<bool> DeleteProductAsync(string organizationId, string productId, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> SearchProductsAsync(string organizationId, string? query, CancellationToken ct = default);
}

public record CreateProductRequest(string SKU, string Barcode, string Name, string? Description, decimal BasePrice, decimal Cost);
public record UpdateProductRequest(string? SKU, string? Barcode, string? Name, string? Description, decimal? BasePrice, decimal? Cost, bool? IsActive);

