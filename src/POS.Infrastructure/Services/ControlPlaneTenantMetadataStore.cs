using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Infrastructure;

namespace POS.Infrastructure.Services;

public class ControlPlaneTenantMetadataStore : ITenantMetadataStore
{
    private readonly ControlPlaneDbContext _controlPlaneDb;
    private readonly IConnectionStringProtector _protector;

    public ControlPlaneTenantMetadataStore(ControlPlaneDbContext controlPlaneDb, IConnectionStringProtector protector)
    {
        _controlPlaneDb = controlPlaneDb;
        _protector = protector;
    }

    public async Task<string?> GetTenantConnectionStringAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        var info = await _controlPlaneDb.TenantConnections
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId, cancellationToken);
        if (info?.ConnectionString == null) return null;
        try
        {
            return _protector.Unprotect(info.ConnectionString);
        }
        catch
        {
            // backward compatibility if stored as plaintext
            return info.ConnectionString;
        }
    }
}

