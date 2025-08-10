namespace POS.Modules.Customers.Services;

public record CustomerAnalyticsDto(decimal TotalSpent, int OrdersCount, decimal AverageOrder, DateTime? LastPurchaseAt, int DaysSinceLastPurchase);