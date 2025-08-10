using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Reporting;

public class ReportingModule : IModule
{
    public string Name => "Reporting";

    public void ConfigureServices(IServiceCollection services)
    {
        // Reporting services will be configured here
        // This is a placeholder for now
    }
}