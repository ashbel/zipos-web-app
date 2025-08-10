using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Customers;

public class CustomersModule : IModule
{
    public string Name => "Customers";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<Services.ICustomerService, Services.CustomerService>();
        services.AddScoped<Services.ICustomerHistoryService, Services.CustomerHistoryService>();
        services.AddScoped<Services.ICustomerLoyaltyService, Services.CustomerLoyaltyService>();
        services.AddScoped<Services.ICustomerAnalyticsService, Services.CustomerAnalyticsService>();
    }
}