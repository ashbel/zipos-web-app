using System.Threading;

namespace POS.Shared.Infrastructure;

public interface ITenantProvisioningService
{
    Task ProvisionTenantAsync(string organizationId, string connectionString, CancellationToken cancellationToken = default);
    Task<bool> TenantExistsAsync(string organizationId, CancellationToken cancellationToken = default);
    Task RemoveTenantAsync(string organizationId, CancellationToken cancellationToken = default);
}

