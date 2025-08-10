using Microsoft.Extensions.Configuration;
using POS.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace POS.Infrastructure.Services;

public class TenantConnectionResolver : ITenantConnectionResolver
{
    private readonly IConfiguration _configuration;
    private readonly ITenantMetadataStore _tenantMetadataStore;

    public TenantConnectionResolver(IConfiguration configuration, ITenantMetadataStore tenantMetadataStore)
    {
        _configuration = configuration;
        _tenantMetadataStore = tenantMetadataStore;
    }

    public string GetConnectionString(string? organizationId)
    {
        return ResolveAsync(organizationId, CancellationToken.None).GetAwaiter().GetResult();
    }

    public Task<string> GetConnectionStringAsync(string? organizationId, CancellationToken cancellationToken = default)
    {
        return ResolveAsync(organizationId, cancellationToken);
    }

    private async Task<string> ResolveAsync(string? organizationId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(organizationId))
        {
            // Control-plane database for system operations
            return _configuration.GetConnectionString("ControlPlaneConnection")
                   ?? _configuration.GetConnectionString("DefaultConnection")
                   ?? throw new InvalidOperationException("Missing control-plane connection string");
        }

        // Look up from control-plane metadata store
        var stored = await _tenantMetadataStore.GetTenantConnectionStringAsync(organizationId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(stored))
        {
            return stored!;
        }

        // Convention-based fallback if not found
        var template = _configuration.GetConnectionString("TenantConnectionTemplate");
        if (!string.IsNullOrWhiteSpace(template))
        {
            return template.Replace("{organizationId}", organizationId);
        }

        // Fallback to DefaultConnection if no template is provided
        var defaultConn = _configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(defaultConn))
        {
            return defaultConn;
        }

        throw new InvalidOperationException($"No connection string found for tenant '{organizationId}'.");
    }
}

