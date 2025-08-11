using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.CurrencyManagement;

public class CurrencyModule : IModule
{
    public string Name => "Currency";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<Services.ICurrencyService, Services.CurrencyService>();
        services.AddScoped<Services.IExchangeRateService, Services.ExchangeRateService>();
    }
}