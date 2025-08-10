using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using POS.Infrastructure.Data;
using POS.Infrastructure.Events;
using POS.Infrastructure.Services;
using POS.Shared.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;

namespace POS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Control-plane and tenant-aware database services
        services.AddScoped<ITenantConnectionResolver, TenantConnectionResolver>();
        services.AddScoped<ITenantMetadataStore, ControlPlaneTenantMetadataStore>();
        services.AddDbContext<ControlPlaneDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("ControlPlaneConnection")
                ?? configuration.GetConnectionString("DefaultConnection")));
        services.AddDbContext<POSDbContext>((provider, options) =>
        {
            var resolver = provider.GetRequiredService<ITenantConnectionResolver>();
            var tenantContext = provider.GetRequiredService<ITenantContext>();
            var connectionString = resolver.GetConnectionString(tenantContext.OrganizationId);
            options.UseNpgsql(connectionString);
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<POSDbContext>());

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(ITenantRepository<>), typeof(TenantRepository<>));

        // Event Bus
        services.AddScoped<IEventBus, InMemoryEventBus>();

        // Tenant Context
        services.AddScoped<ITenantContext, TenantContext>();
        
        // Schema Management (deprecated) → Database provisioning service could replace this
        // services.AddScoped<ISchemaService, SchemaService>();

        // Caching
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        });
        services.AddScoped<ICacheService, RedisCacheService>();

        // Background Jobs
        services.AddHangfire(config =>
        {
            // Use control-plane DB for background jobs to be tenant-agnostic
            var hangfireConn = configuration.GetConnectionString("ControlPlaneConnection")
                               ?? configuration.GetConnectionString("DefaultConnection");
            config.UsePostgreSqlStorage(hangfireConn);
        });
        services.AddHangfireServer();
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }

    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<POSDbContext>();
        await SeedData.SeedAsync(context);
    }
}