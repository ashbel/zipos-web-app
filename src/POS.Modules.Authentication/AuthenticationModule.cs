using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Authentication;

public class AuthenticationModule : IModule
{
    public string Name => "Authentication";

    public void ConfigureServices(IServiceCollection services)
    {
        // Authentication services will be configured here
        // This is a placeholder for now
    }
}