using POS.Shared.Domain;

namespace POS.Modules.Inventory.Services;

public interface ISupplierService
{
    Task<Supplier> CreateSupplierAsync(string organizationId, CreateSupplierRequest request, CancellationToken ct = default);
    Task<Supplier?> UpdateSupplierAsync(string organizationId, string supplierId, UpdateSupplierRequest request, CancellationToken ct = default);
    Task<bool> DeleteSupplierAsync(string organizationId, string supplierId, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> SearchSuppliersAsync(string organizationId, string? q, CancellationToken ct = default);
}

public record CreateSupplierRequest(string Name, string? Email, string? Phone, string? PaymentTerms, string? Address);
public record UpdateSupplierRequest(string? Name, string? Email, string? Phone, string? PaymentTerms, string? Address, bool? IsActive);

