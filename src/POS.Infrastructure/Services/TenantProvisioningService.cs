using Microsoft.EntityFrameworkCore;
using Npgsql;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Infrastructure;
using POS.Shared.Domain.Events;
using System.Security.Cryptography;
using System.Text;

namespace POS.Infrastructure.Services;

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly ControlPlaneDbContext _controlPlaneDbContext;
    private readonly IConnectionStringProtector _protector;

    public TenantProvisioningService(ControlPlaneDbContext controlPlaneDbContext, IConnectionStringProtector protector)
    {
        _controlPlaneDbContext = controlPlaneDbContext;
        _protector = protector;
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
            await SeedTenantDefaultsAsync(tenantDb, cancellationToken);
        }

        // 3) Store/Upsert connection string in control-plane
        var existing = await _controlPlaneDbContext.TenantConnections
            .FirstOrDefaultAsync(x => x.OrganizationId == organizationId, cancellationToken);
        var protectedConn = _protector.Protect(connectionString);
        if (existing == null)
        {
            _controlPlaneDbContext.TenantConnections.Add(new TenantConnectionInfo
            {
                OrganizationId = organizationId,
                ConnectionString = protectedConn,
                Provider = "Npgsql",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.ConnectionString = protectedConn;
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

    private static async Task SeedTenantDefaultsAsync(POSDbContext tenantDb, CancellationToken cancellationToken)
    {
        // Seed roles
        var adminRole = await tenantDb.Roles.FirstOrDefaultAsync(r => r.Name == "Admin", cancellationToken);
        if (adminRole == null)
        {
            adminRole = new Role { Name = "Admin", Description = "System Administrator", IsSystemRole = true };
            await tenantDb.Roles.AddAsync(adminRole, cancellationToken);
        }

        var managerRole = await tenantDb.Roles.FirstOrDefaultAsync(r => r.Name == "Manager", cancellationToken) ?? new Role { Name = "Manager", Description = "Branch Manager" };
        if (managerRole.Id == null || managerRole.Id == string.Empty)
        {
            await tenantDb.Roles.AddAsync(managerRole, cancellationToken);
        }

        var cashierRole = await tenantDb.Roles.FirstOrDefaultAsync(r => r.Name == "Cashier", cancellationToken) ?? new Role { Name = "Cashier", Description = "Cashier" };
        if (cashierRole.Id == null || cashierRole.Id == string.Empty)
        {
            await tenantDb.Roles.AddAsync(cashierRole, cancellationToken);
        }

        await tenantDb.SaveChangesAsync(cancellationToken);

        // Seed admin user
        var adminEmail = "admin@tenant.local";
        var admin = await tenantDb.Users.FirstOrDefaultAsync(u => u.Email == adminEmail, cancellationToken);
        if (admin == null)
        {
            admin = new User
            {
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                PasswordHash = ComputeSha256("ChangeMe!123"),
                IsActive = true,
                EmailConfirmed = true
            };
            await tenantDb.Users.AddAsync(admin, cancellationToken);
            await tenantDb.SaveChangesAsync(cancellationToken);

            // Link admin user to Admin role
            var userRole = new UserRole { UserId = admin.Id, RoleId = adminRole.Id };
            await tenantDb.UserRoles.AddAsync(userRole, cancellationToken);
        }

        await tenantDb.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
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
        public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : IDomainEvent
            => Task.CompletedTask;

        public Task PublishAsync(IDomainEvent @event, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
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

