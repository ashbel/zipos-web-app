using POS.Shared.Domain;

namespace POS.Modules.Customers.Services;

public interface ICustomerHistoryService
{
    Task<IReadOnlyList<Sale>> GetPurchaseHistoryAsync(string organizationId, string customerId, CancellationToken ct = default);
}

