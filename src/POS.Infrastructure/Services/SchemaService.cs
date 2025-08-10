using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Infrastructure;

namespace POS.Infrastructure.Services;

// Deprecated in database-per-tenant architecture. Kept temporarily for backward compatibility.
public class SchemaService : ISchemaService
{
    private readonly POSDbContext _context;

    public SchemaService(POSDbContext context)
    {
        _context = context;
    }

    public async Task CreateTenantSchemaAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        var schemaName = GetSchemaName(organizationId);
        
        // Validate schema name to prevent SQL injection
        if (!IsValidSchemaName(schemaName))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}");
        }
        
        // Create the schema (using raw SQL since schema names can't be parameterized)
        await _context.Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\"", cancellationToken);
        
        // Create tenant-specific tables in the new schema
        await CreateTenantTablesAsync(schemaName, cancellationToken);
    }

    public async Task DeleteTenantSchemaAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        var schemaName = GetSchemaName(organizationId);
        
        // Validate schema name to prevent SQL injection
        if (!IsValidSchemaName(schemaName))
        {
            throw new ArgumentException($"Invalid schema name: {schemaName}");
        }
        
        // Drop the entire schema and all its objects (using raw SQL since schema names can't be parameterized)
        await _context.Database.ExecuteSqlRawAsync($"DROP SCHEMA IF EXISTS \"{schemaName}\" CASCADE", cancellationToken);
    }

    public async Task<bool> SchemaExistsAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        var schemaName = GetSchemaName(organizationId);
        
        var exists = await _context.Database.SqlQueryRaw<bool>(
            "SELECT EXISTS(SELECT 1 FROM information_schema.schemata WHERE schema_name = {0})",
            schemaName).FirstOrDefaultAsync(cancellationToken);
            
        return exists;
    }

    public async Task MigrateTenantSchemaAsync(string organizationId, CancellationToken cancellationToken = default)
    {
        var schemaName = GetSchemaName(organizationId);
        
        if (!await SchemaExistsAsync(organizationId, cancellationToken))
        {
            await CreateTenantSchemaAsync(organizationId, cancellationToken);
        }
        else
        {
            // Apply any schema updates/migrations
            await UpdateTenantSchemaAsync(schemaName, cancellationToken);
        }
    }

    public string GetSchemaName(string organizationId)
    {
        return $"org_{organizationId}";
    }

    private static bool IsValidSchemaName(string schemaName)
    {
        // Validate schema name to prevent SQL injection
        // Schema names should only contain alphanumeric characters and underscores
        return !string.IsNullOrEmpty(schemaName) && 
               schemaName.All(c => char.IsLetterOrDigit(c) || c == '_') &&
               schemaName.StartsWith("org_");
    }

    private async Task CreateTenantTablesAsync(string schemaName, CancellationToken cancellationToken)
    {
        // Create tenant-specific tables
        var createTablesScript = $@"
            -- Branches table
            CREATE TABLE ""{schemaName}"".""branches"" (
                ""Id"" text NOT NULL,
                ""Name"" character varying(200) NOT NULL,
                ""Code"" character varying(20) NOT NULL,
                ""ContactPhone"" character varying(50),
                ""ContactEmail"" character varying(255),
                ""IsActive"" boolean NOT NULL DEFAULT true,
                ""TimeZone"" character varying(50) NOT NULL DEFAULT 'UTC',
                ""Currency"" character varying(3) NOT NULL DEFAULT 'USD',
                ""TaxRate"" numeric(5,4) NOT NULL DEFAULT 0,
                ""TaxInclusive"" boolean NOT NULL DEFAULT false,
                ""Settings"" jsonb NOT NULL DEFAULT '{{}}',
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""CreatedBy"" text,
                ""UpdatedBy"" text,
                ""IsDeleted"" boolean NOT NULL DEFAULT false,
                ""DeletedAt"" timestamp with time zone,
                ""DeletedBy"" text,
                CONSTRAINT ""PK_branches"" PRIMARY KEY (""Id"")
            );

            -- Users table
            CREATE TABLE ""{schemaName}"".""users"" (
                ""Id"" text NOT NULL,
                ""Email"" character varying(255) NOT NULL,
                ""FirstName"" character varying(100) NOT NULL,
                ""LastName"" character varying(100) NOT NULL,
                ""PasswordHash"" character varying(500) NOT NULL,
                ""PhoneNumber"" character varying(50),
                ""IsActive"" boolean NOT NULL DEFAULT true,
                ""EmailConfirmed"" boolean NOT NULL DEFAULT false,
                ""LastLoginAt"" timestamp with time zone,
                ""RefreshToken"" character varying(500),
                ""RefreshTokenExpiresAt"" timestamp with time zone,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""CreatedBy"" text,
                ""UpdatedBy"" text,
                ""IsDeleted"" boolean NOT NULL DEFAULT false,
                ""DeletedAt"" timestamp with time zone,
                ""DeletedBy"" text,
                CONSTRAINT ""PK_users"" PRIMARY KEY (""Id"")
            );

            -- Roles table
            CREATE TABLE ""{schemaName}"".""roles"" (
                ""Id"" text NOT NULL,
                ""Name"" character varying(100) NOT NULL,
                ""Description"" character varying(500),
                ""IsSystemRole"" boolean NOT NULL DEFAULT false,
                ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""UpdatedAt"" timestamp with time zone NOT NULL DEFAULT now(),
                ""CreatedBy"" text,
                ""UpdatedBy"" text,
                ""IsDeleted"" boolean NOT NULL DEFAULT false,
                ""DeletedAt"" timestamp with time zone,
                ""DeletedBy"" text,
                CONSTRAINT ""PK_roles"" PRIMARY KEY (""Id"")
            );

            -- Create indexes
            CREATE UNIQUE INDEX ""IX_branches_Code"" ON ""{schemaName}"".""branches"" (""Code"");
            CREATE INDEX ""IX_branches_Name"" ON ""{schemaName}"".""branches"" (""Name"");
            CREATE UNIQUE INDEX ""IX_users_Email"" ON ""{schemaName}"".""users"" (""Email"");
            CREATE UNIQUE INDEX ""IX_roles_Name"" ON ""{schemaName}"".""roles"" (""Name"");
        ";

        await _context.Database.ExecuteSqlRawAsync(createTablesScript, cancellationToken);
    }

    private Task UpdateTenantSchemaAsync(string schemaName, CancellationToken cancellationToken)
    {
        // Apply any schema updates/migrations for existing tenant schemas
        // This would contain ALTER TABLE statements for schema evolution
        
        // Example: Add new columns, indexes, etc.
        // await _context.Database.ExecuteSqlRawAsync($"ALTER TABLE \"{schemaName}\".\"users\" ADD COLUMN IF NOT EXISTS \"NewColumn\" text", cancellationToken);
        
        return Task.CompletedTask;
    }
}