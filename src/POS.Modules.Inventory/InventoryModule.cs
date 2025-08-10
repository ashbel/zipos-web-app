using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Inventory;

public class InventoryModule : IModule
{
    public string Name => "Inventory";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<Services.IProductService, Services.ProductService>();
        services.AddScoped<Services.IInventoryService, Services.InventoryService>();
    }
}