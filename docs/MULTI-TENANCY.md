# Multi-Tenancy Architecture: Single Database, Separate Schemas

## Overview

The Modern POS System implements a **Single Database, Separate Schemas** multi-tenancy pattern, providing complete data isolation between organizations while maintaining operational simplicity.

## Architecture Decision

### Why Schema-Based Multi-Tenancy?

We chose schema-based multi-tenancy over other approaches for the following reasons:

#### ✅ **Advantages**

1. **Complete Data Isolation**
   - Physical separation at the database schema level
   - No risk of cross-tenant data access
   - Natural security boundary

2. **Better Performance**
   - No need for `OrganizationId` filtering in queries
   - Simpler query plans and better index utilization
   - Reduced query complexity

3. **Easier Data Management**
   - Schema-level operations (backup, restore, migration)
   - Individual tenant data can be managed independently
   - Easier compliance with data residency requirements

4. **Scalability**
   - Each tenant can have different indexing strategies
   - Schema-specific optimizations possible
   - Better resource isolation

5. **Future-Proof**
   - Easy extraction to separate databases for microservices
   - Natural boundary for tenant-specific customizations
   - Simplified data migration strategies

#### ⚠️ **Trade-offs**

1. **Schema Management Complexity**
   - Need to manage multiple schemas
   - Schema creation/deletion operations required
   - Migration complexity across multiple schemas

2. **Database Connection Overhead**
   - Need to switch schema context per request
   - Slightly more complex connection management

3. **Cross-Tenant Queries**
   - More complex to implement system-wide analytics
   - Requires explicit cross-schema queries

## Implementation Details

### Schema Structure

```
PostgreSQL Database: modernpos
├── public (System Schema)
│   ├── organizations          # Organization registry
│   ├── system_permissions     # Global permissions
│   ├── system_settings        # System configuration
│   └── audit_logs            # Cross-tenant audit trail
│
├── org_12345 (Tenant Schema)
│   ├── branches              # Organization branches
│   ├── users                 # Organization users
│   ├── roles                 # Organization roles
│   ├── products              # Product catalog
│   ├── inventory             # Stock levels
│   ├── sales                 # Transaction data
│   ├── customers             # Customer data
│   └── ...                   # Other tenant data
│
└── org_67890 (Another Tenant Schema)
    └── ... (same structure)
```

### Schema Naming Convention

- **System Schema**: `public`
- **Tenant Schema**: `org_{organizationId}`
- **Example**: Organization with ID "12345" gets schema `org_12345`

### Dynamic Schema Resolution

The system automatically resolves the correct schema based on the tenant context:

```csharp
public class POSDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set default schema based on tenant context
        var schema = GetSchemaForTenant();
        if (!string.IsNullOrEmpty(schema))
        {
            modelBuilder.HasDefaultSchema(schema);
        }
        
        // Configure schema-specific entities
        ConfigureSchemaSpecificEntities(modelBuilder, schema);
    }

    private string GetSchemaForTenant()
    {
        var organizationId = _tenantContext.OrganizationId;
        return string.IsNullOrEmpty(organizationId) 
            ? "public" 
            : $"org_{organizationId}";
    }
}
```

## Data Model Changes

### Removed OrganizationId Fields

With schema-based isolation, we no longer need `OrganizationId` fields in tenant entities:

**Before (Row-Level Security):**
```csharp
public class User : TenantEntity
{
    public string OrganizationId { get; set; } // ❌ No longer needed
    public string Email { get; set; }
    // ... other properties
}
```

**After (Schema-Based):**
```csharp
public class User : TenantEntity
{
    // OrganizationId removed - schema provides isolation
    public string Email { get; set; }
    // ... other properties
}
```

### Simplified Queries

**Before (Row-Level Security):**
```csharp
// Had to filter by OrganizationId
var users = await _context.Users
    .Where(u => u.OrganizationId == organizationId)
    .ToListAsync();
```

**After (Schema-Based):**
```csharp
// Schema context automatically isolates data
var users = await _context.Users.ToListAsync();
```

## Schema Management

### ISchemaService Interface

```csharp
public interface ISchemaService
{
    Task CreateTenantSchemaAsync(string organizationId, CancellationToken cancellationToken = default);
    Task DeleteTenantSchemaAsync(string organizationId, CancellationToken cancellationToken = default);
    Task<bool> SchemaExistsAsync(string organizationId, CancellationToken cancellationToken = default);
    Task MigrateTenantSchemaAsync(string organizationId, CancellationToken cancellationToken = default);
    string GetSchemaName(string organizationId);
}
```

### Tenant Onboarding Process

1. **Organization Registration**
   ```csharp
   // 1. Create organization record in public schema
   var organization = new Organization { Name = "Acme Corp" };
   await _organizationRepository.AddAsync(organization);
   
   // 2. Create dedicated schema for the organization
   await _schemaService.CreateTenantSchemaAsync(organization.Id);
   
   // 3. Create default admin user in tenant schema
   using var tenantContext = _tenantContextFactory.CreateContext(organization.Id);
   var adminUser = new User { Email = "admin@acme.com" };
   await _userRepository.AddAsync(adminUser);
   ```

2. **Schema Creation**
   - Creates schema with naming convention `org_{organizationId}`
   - Creates all tenant-specific tables
   - Sets up indexes and constraints
   - Applies initial data seeding

### Tenant Offboarding Process

1. **Data Export** (if required)
2. **Schema Deletion**
   ```csharp
   await _schemaService.DeleteTenantSchemaAsync(organizationId);
   ```
3. **Organization Record Cleanup**

## Security Considerations

### Schema-Level Security

1. **Database User Permissions**
   - Application database user has access to all schemas
   - Schema isolation enforced at application level
   - No direct database access for tenants

2. **Application-Level Security**
   - Tenant context validation on every request
   - Schema switching based on authenticated user's organization
   - Audit logging for all schema operations

3. **SQL Injection Prevention**
   - Schema names validated against whitelist pattern
   - Parameterized queries where possible
   - Input sanitization for schema operations

### Access Control Flow

```
User Request → Authentication → Tenant Resolution → Schema Context → Data Access
```

1. **Authentication**: Verify user identity
2. **Tenant Resolution**: Determine user's organization
3. **Schema Context**: Set database schema context
4. **Data Access**: Execute queries in tenant schema

## Migration Strategy

### From Row-Level Security to Schema-Based

1. **Phase 1: Preparation**
   - ✅ Update domain models (remove OrganizationId)
   - ✅ Update Entity Framework configurations
   - ✅ Implement schema service
   - ✅ Create new migration

2. **Phase 2: Data Migration** (Future)
   - Export existing tenant data
   - Create tenant schemas
   - Import data into respective schemas
   - Validate data integrity

3. **Phase 3: Cleanup** (Future)
   - Remove old OrganizationId columns
   - Update application logic
   - Performance testing and optimization

### Database Migration Commands

```bash
# Create new migration
dotnet ef migrations add SchemaBasedMultiTenancy

# Apply migration
dotnet ef database update

# Create tenant schema (via application)
POST /api/organizations/{id}/schema
```

## Performance Implications

### Query Performance

**Before (Row-Level Security):**
```sql
-- Every query needed OrganizationId filter
SELECT * FROM users WHERE organization_id = '12345' AND email = 'user@example.com';
```

**After (Schema-Based):**
```sql
-- Direct access within schema context
SET search_path TO org_12345;
SELECT * FROM users WHERE email = 'user@example.com';
```

### Index Strategy

**Before:**
- Composite indexes: `(organization_id, email)`, `(organization_id, created_at)`
- Larger index sizes due to organization_id inclusion

**After:**
- Simple indexes: `(email)`, `(created_at)`
- Smaller, more efficient indexes per schema
- Schema-specific optimization possible

## Monitoring and Observability

### Schema-Level Metrics

1. **Schema Count**: Number of active tenant schemas
2. **Schema Size**: Storage usage per tenant
3. **Query Performance**: Response times per schema
4. **Connection Usage**: Database connections per tenant

### Logging Strategy

```csharp
// Log schema operations
_logger.LogInformation("Creating schema for organization {OrganizationId}", organizationId);

// Log tenant context switches
_logger.LogDebug("Switching to schema {SchemaName} for user {UserId}", schemaName, userId);
```

## Best Practices

### Development Guidelines

1. **Always Use Tenant Context**
   ```csharp
   // ✅ Good - Uses tenant context
   using var scope = _tenantContextFactory.CreateScope(organizationId);
   var users = await _userRepository.GetAllAsync();
   
   // ❌ Bad - Direct schema access
   var users = await _context.Database.SqlQuery<User>("SELECT * FROM org_12345.users");
   ```

2. **Schema Name Validation**
   ```csharp
   private static bool IsValidSchemaName(string schemaName)
   {
       return !string.IsNullOrEmpty(schemaName) && 
              schemaName.All(c => char.IsLetterOrDigit(c) || c == '_') &&
              schemaName.StartsWith("org_");
   }
   ```

3. **Error Handling**
   ```csharp
   try
   {
       await _schemaService.CreateTenantSchemaAsync(organizationId);
   }
   catch (PostgresException ex) when (ex.SqlState == "42P06") // duplicate_schema
   {
       _logger.LogWarning("Schema already exists for organization {OrganizationId}", organizationId);
   }
   ```

### Testing Strategy

1. **Unit Tests**: Mock tenant context and schema service
2. **Integration Tests**: Test with multiple schemas
3. **Performance Tests**: Validate query performance across schemas

## Future Enhancements

### Planned Improvements

1. **Schema Versioning**
   - Track schema version per tenant
   - Support for gradual schema migrations
   - Rollback capabilities

2. **Cross-Tenant Analytics**
   - Aggregated reporting across all tenants
   - System-wide metrics and insights
   - Compliance reporting

3. **Schema Optimization**
   - Tenant-specific indexing strategies
   - Automated performance tuning
   - Storage optimization

4. **Microservices Migration**
   - Extract schemas to separate databases
   - Service-per-tenant architecture
   - Event-driven synchronization

This schema-based multi-tenancy approach provides a solid foundation for the Modern POS System, balancing data isolation, performance, and operational simplicity while maintaining flexibility for future architectural evolution.