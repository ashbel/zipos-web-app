using Microsoft.Extensions.DependencyInjection;
using POS.Shared.Infrastructure;

namespace POS.Modules.Authentication;

public class AuthenticationModule : IModule
{
    public string Name => "Authentication";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<Services.IAuthenticationService, Services.JwtAuthenticationService>();
    }
}