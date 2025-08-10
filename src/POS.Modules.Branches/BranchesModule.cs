using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Branches;

public class BranchesModule : IModule
{
    public string Name => "Branches";

    public void ConfigureServices(IServiceCollection services)
    {
        // Branch services will be configured here
        // This is a placeholder for now
    }
}