using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Infrastructure;

namespace POS.Infrastructure.Services;

public class OutOfDateTenantMigrationJob
{
    private readonly IServiceProvider _services;
    private readonly ITenantMetadataStore _tenantMetadataStore;
    private readonly ITenantContext _tenantContext;

    public OutOfDateTenantMigrationJob(IServiceProvider services, ITenantMetadataStore tenantMetadataStore, ITenantContext tenantContext)
    {
        _services = services;
        _tenantMetadataStore = tenantMetadataStore;
        _tenantContext = tenantContext;
    }

    public async Task RunAsync(string organizationId, CancellationToken ct = default)
    {
        using var scope = _services.CreateScope();
        _tenantContext.SetTenant(organizationId);
        var db = scope.ServiceProvider.GetRequiredService<POSDbContext>();
        await db.Database.MigrateAsync(ct);
    }
}


