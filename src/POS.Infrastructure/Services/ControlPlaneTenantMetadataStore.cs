using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Infrastructure;

namespace POS.Infrastructure.Services;

public class ControlPlaneTenantMetadataStore : ITenantMetadataStore
{
    private readonly ControlPlaneDbContext _controlPlaneDb;

    public ControlPlaneTenantMetadataStore(ControlPlaneDbContext controlPlaneDb)
    {
        _controlPlaneDb = controlPlaneDb;
    }

    public async Task<string?> GetTenantConnectionStringAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        var info = await _controlPlaneDb.TenantConnections
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId, cancellationToken);
        return info?.ConnectionString;
    }
}

