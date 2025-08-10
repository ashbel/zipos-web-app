using Microsoft.Extensions.DependencyInjection;

namespace POS.Shared.Infrastructure;

public interface IModule
{
    string Name { get; }
    void ConfigureServices(IServiceCollection services);
}