using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Promotions;

public class PromotionsModule : IModule
{
    public string Name => "Promotions";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<Services.IPromotionService, Services.PromotionService>();
    }
}