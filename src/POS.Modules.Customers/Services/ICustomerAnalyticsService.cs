namespace POS.Modules.Customers.Services;

public interface ICustomerAnalyticsService
{
    Task<CustomerAnalyticsDto> GetAnalyticsAsync(string organizationId, string customerId, CancellationToken ct = default);
}