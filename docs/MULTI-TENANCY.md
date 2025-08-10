# Multi-Tenancy Architecture: Separate Database per Tenant

## Overview

The Modern POS System implements a **Separate Database per Tenant** pattern. Each organization (tenant) is provisioned with its own dedicated database instance. A small, shared control-plane database stores tenant metadata and connection information.

## Architecture Decision

### Why Database-per-Tenant?

#### ✅ Advantages

1. **Strongest Isolation and Security**
   - Complete physical isolation of data per tenant
   - Minimizes risk of cross-tenant data access
   - Natural compliance boundary (PII segregation, legal holds)

2. **Performance and Scalability Isolation**
   - No noisy-neighbor contention across tenants
   - Scale up/down or move heavy tenants independently
   - Per-tenant read replicas and performance tuning

3. **Operations and Compliance Flexibility**
   - Per-tenant backup/restore, retention, and DR plans
   - Region-specific placement for data residency
   - Tenant-specific DB extensions and settings

4. **Customization Readiness**
   - Safe to diverge schema/indexes for premium tenants
   - Simplifies future migration to tenant-dedicated services

#### ⚠️ Trade-offs

1. **Operational Overhead**
   - Provisioning, lifecycle, and cost management for many databases
   - Requires automation for creation, migration, seeding, and deletion

2. **Migrations & Version Skew**
   - Rolling out schema changes per tenant
   - Handling tenants temporarily on different versions

3. **Connection & Resource Management**
   - More connection pools (one per active tenant)
   - Higher aggregate idle resource footprint

4. **Cross-Tenant Analytics**
   - Requires aggregating data into a warehouse or using fan-out queries

## Implementation Details

### Topology

```
PostgreSQL Cluster(s)
├── modernpos_control (Control Plane DB)
│   ├── organizations              # Tenant registry
│   ├── tenant_connection_strings  # Encrypted DSNs per tenant
│   └── audit_logs                 # System-wide operations audit
│
├── modernpos_org_12345 (Tenant DB)
│   ├── branches
│   ├── users
│   ├── roles
│   ├── products
│   ├── inventory
│   ├── sales
│   ├── customers
│   └── …
│
└── modernpos_org_67890 (Another Tenant DB)
    └── … (same structure)
```

### Naming Convention

- Control-plane database: `modernpos_control`
- Tenant database: `modernpos_org_{organizationId}`

### Dynamic Connection Resolution

At the start of each request, resolve the active tenant and fetch its connection string from the control-plane database or a secure secret store. Create the `DbContext` with the tenant-specific connection string.

```csharp
public interface ITenantConnectionResolver
{
    Task<string> GetConnectionStringAsync(string organizationId, CancellationToken ct = default);
}

public class POSDbContextFactory : IDbContextFactory<POSDbContext>
{
    private readonly ITenantConnectionResolver _resolver;
    private readonly ITenantContext _tenantContext;

    public POSDbContextFactory(ITenantConnectionResolver resolver, ITenantContext tenantContext)
    {
        _resolver = resolver;
        _tenantContext = tenantContext;
    }

    public async Task<POSDbContext> CreateDbContextAsync(CancellationToken ct = default)
    {
        var organizationId = _tenantContext.OrganizationId
            ?? throw new InvalidOperationException("Missing tenant context");

        var connectionString = await _resolver.GetConnectionStringAsync(organizationId, ct);

        var options = new DbContextOptionsBuilder<POSDbContext>()
            .UseNpgsql(connectionString)
            .EnableSensitiveDataLogging(false)
            .Options;

        return new POSDbContext(options);
    }
}
```

### Tenant Lifecycle (Onboarding/Offboarding)

1. **Onboarding**
   - Create organization record in control-plane DB
   - Provision dedicated tenant database (name using convention)
   - Apply EF Core migrations to the tenant DB
   - Seed default data (admin user, roles, settings)
   - Store encrypted connection string and metadata

2. **Offboarding**
   - Export tenant data if requested
   - Disable access and revoke credentials
   - Snapshot/backup and delete tenant database as per policy
   - Remove connection metadata from control-plane

### Migrations Strategy

- Single migrations assembly; apply per tenant database
- Operational job runs migrations for newly onboarded or out-of-date tenants
- Track per-tenant schema version in each tenant database

### Connection Pooling

- Use short-lived contexts and per-request scope
- Configure max pool size based on expected concurrent tenants per node
- Cache connection strings securely with expiration

## Security Considerations

1. **Least Privilege**
   - Prefer per-tenant DB users with access limited to their DB only
   - Rotate credentials and store in a secret manager (e.g., Azure Key Vault)

2. **Input/Name Validation**
   - Validate organization IDs and database names against strict patterns

3. **Audit & Monitoring**
   - Centralized audit of tenant lifecycle events in control-plane DB
   - Per-tenant audit logs inside each tenant DB

4. **Network & Compliance**
   - Optional VNet/service endpoints per large tenant
   - Region pinning for data residency

## Migration Path (from Schema-per-Tenant)

1. Freeze writes per tenant during cutover (or implement dual-write with reconciliation)
2. Export data from schema `org_{id}`
3. Create `modernpos_org_{id}` DB and apply migrations
4. Import data; verify integrity and counts
5. Update control-plane with tenant connection string
6. Switch application routing to new DB; decommission old schema

## Performance Implications

- Isolation removes cross-tenant contention
- Heavier tenants can be moved to dedicated hardware/cluster
- Ensure connection pool sizing matches workload per node

## Monitoring and Observability

### Control-Plane Metrics
- Active tenant count, onboard/offboard rates
- Tenant migration backlog and failures

### Per-Tenant Metrics
- Database size and growth
- Query performance and error rates
- Connection utilization and timeouts

## Best Practices

1. Always resolve tenant before creating `DbContext`
2. Avoid cross-tenant transactions; use outbox/events for cross-tenant workflows
3. Keep migrations backward compatible where feasible; roll out gradually
4. Maintain runbooks for provisioning, restore, and incident response per tenant

This database-per-tenant approach maximizes isolation, performance, and compliance flexibility while requiring solid operational automation for provisioning, migrations, and monitoring.