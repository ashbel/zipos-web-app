using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace POS.Shared.Infrastructure;

public static class ModuleExtensions
{
    public static IServiceCollection AddModules(this IServiceCollection services, params Assembly[] assemblies)
    {
        var modules = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IModule).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
            .Select(Activator.CreateInstance)
            .Cast<IModule>()
            .ToList();

        foreach (var module in modules)
        {
            module.ConfigureServices(services);
        }

        return services;
    }
}