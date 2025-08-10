using POS.Shared.Domain;

namespace POS.Modules.Customers.Services;

public interface ICustomerLoyaltyService
{
    Task<CustomerLoyalty> GetAsync(string organizationId, string customerId, CancellationToken ct = default);
    Task<CustomerLoyalty> AddPointsAsync(string organizationId, string customerId, int points, CancellationToken ct = default);
}

