using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Inventory;

public class InventoryModule : IModule
{
    public string Name => "Inventory";

    public void ConfigureServices(IServiceCollection services)
    {
        // Inventory services will be configured here
        // This is a placeholder for now
    }
}