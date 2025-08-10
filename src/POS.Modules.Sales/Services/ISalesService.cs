using POS.Shared.Domain;

namespace POS.Modules.Sales.Services;

public interface ISalesService
{
    Task<Cart> CreateCartAsync(string organizationId, string userId, string branchId, CancellationToken ct = default);
    Task<Cart> AddItemAsync(string organizationId, string cartId, string productId, string name, decimal quantity, decimal unitPrice, decimal discount, CancellationToken ct = default);
    Task<Cart> RemoveItemAsync(string organizationId, string cartId, string itemId, CancellationToken ct = default);
    Task<Sale> CheckoutAsync(string organizationId, string cartId, CancellationToken ct = default);
}

