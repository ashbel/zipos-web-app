using System.Threading;

namespace POS.Shared.Infrastructure;

public interface ITenantMetadataStore
{
    Task<string?> GetTenantConnectionStringAsync(string organizationId, CancellationToken cancellationToken = default);
}

