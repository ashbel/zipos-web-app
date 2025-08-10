using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Customers;

public class CustomersModule : IModule
{
    public string Name => "Customers";

    public void ConfigureServices(IServiceCollection services)
    {
        // Customer services will be configured here
        // This is a placeholder for now
    }
}