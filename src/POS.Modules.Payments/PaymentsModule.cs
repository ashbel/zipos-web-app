using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Payments;

public class PaymentsModule : IModule
{
    public string Name => "Payments";

    public void ConfigureServices(IServiceCollection services)
    {
        // Payment services will be configured here
        // This is a placeholder for now
    }
}