using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Sales;

public class SalesModule : IModule
{
    public string Name => "Sales";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<Services.ISalesService, Services.SalesService>();
        services.AddScoped<Services.IReceiptService, Services.ReceiptService>();
    }
}