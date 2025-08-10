using Microsoft.EntityFrameworkCore;
using Npgsql;
using POS.Infrastructure.Data;
using POS.Shared.Infrastructure;

namespace POS.Infrastructure.Services;

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly ControlPlaneDbContext _controlPlaneDbContext;

    public TenantProvisioningService(ControlPlaneDbContext controlPlaneDbContext)
    {
        _controlPlaneDbContext = controlPlaneDbContext;
    }

    public async Task ProvisionTenantAsync(string organizationId, string connectionString, CancellationToken cancellationToken = default)
    {
        // 1) Create database if it does not exist (PostgreSQL-specific)
        await EnsureDatabaseExistsAsync(connectionString, cancellationToken);

        // 2) Apply tenant migrations
        var options = new DbContextOptionsBuilder<POSDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        await using (var tenantDb = new POSDbContext(options, eventBus: new NoopEventBus(), tenantContext: new StaticTenantContext(organizationId)))
        {
            await tenantDb.Database.MigrateAsync(cancellationToken);
        }

        // 3) Store/Upsert connection string in control-plane
        var existing = await _controlPlaneDbContext.TenantConnections
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId, cancellationToken);
        if (existing == null)
        {
            _controlPlaneDbContext.TenantConnections.Add(new TenantConnectionInfo
            {
                OrganizationId = organizationId,
                ConnectionString = connectionString, // encryption handled at store read path
                Provider = "Npgsql",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.ConnectionString = connectionString;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _controlPlaneDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> TenantExistsAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        return await _controlPlaneDbContext.TenantConnections
            .AsNoTracking()
            .AnyAsync(x => x.OrganizationId == organizationId, cancellationToken);
    }

    public async Task RemoveTenantAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        var record = await _controlPlaneDbContext.TenantConnections
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId, cancellationToken);
        if (record != null)
        {
            _controlPlaneDbContext.TenantConnections.Remove(record);
            await _controlPlaneDbContext.SaveChangesAsync(cancellationToken);
        }
        // Note: dropping tenant DB is an operational decision and not done automatically here
    }

    private static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken cancellationToken)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;

        // Connect to postgres maintenance DB to create the target DB if missing
        var adminBuilder = new NpgsqlConnectionStringBuilder(connectionString) { Database = "postgres" };
        await using var admin = new NpgsqlConnection(adminBuilder.ToString());
        await admin.OpenAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname = @dbname", admin);
        cmd.Parameters.AddWithValue("@dbname", databaseName);
        var exists = await cmd.ExecuteScalarAsync(cancellationToken) != null;

        if (!exists)
        {
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin);
            await create.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private sealed class NoopEventBus : IEventBus
    {
        public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StaticTenantContext : ITenantContext
    {
        public StaticTenantContext(string organizationId) { OrganizationId = organizationId; }
        public string? OrganizationId { get; private set; }
        public string? BranchId => null;
        public string? UserId => null;
        public void SetTenant(string organizationId, string? branchId = null, string? userId = null) { OrganizationId = organizationId; }
    }
}

