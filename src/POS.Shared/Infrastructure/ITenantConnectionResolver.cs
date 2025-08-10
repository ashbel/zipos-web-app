using System.Threading;

namespace POS.Shared.Infrastructure;

public interface ITenantConnectionResolver
{
    string GetConnectionString(string? organizationId);
    Task<string> GetConnectionStringAsync(string? organizationId, CancellationToken cancellationToken = default);
}

