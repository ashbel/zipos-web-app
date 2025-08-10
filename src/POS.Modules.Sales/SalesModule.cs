using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Sales;

public class SalesModule : IModule
{
    public string Name => "Sales";

    public void ConfigureServices(IServiceCollection services)
    {
        // Sales services will be configured here
        // This is a placeholder for now
    }
}