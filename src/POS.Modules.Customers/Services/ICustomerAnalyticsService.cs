namespace POS.Modules.Customers.Services;

public interface ICustomerAnalyticsService
{
    Task<CustomerAnalyticsDto> GetAnalyticsAsync(string organizationId, string customerId, CancellationToken ct = default);
}

public record CustomerAnalyticsDto(decimal TotalSpent, int OrdersCount, decimal AverageOrder, DateTime? LastPurchaseAt, int DaysSinceLastPurchase);

