using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Recipes;

public class RecipesModule : IModule
{
    public string Name => "Recipes";

    public void ConfigureServices(IServiceCollection services)
    {
        // Recipe services will be configured here
        // This is a placeholder for now
    }
}