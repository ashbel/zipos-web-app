using Microsoft.EntityFrameworkCore;
using POS.Shared.Domain;
using POS.Shared.Domain.Events;
using POS.Shared.Infrastructure;
using System.Linq.Expressions;

namespace POS.Infrastructure.Data;

public class POSDbContext : DbContext, IUnitOfWork
{
    private readonly IEventBus _eventBus;
    private readonly ITenantContext _tenantContext;
    
    public POSDbContext(DbContextOptions<POSDbContext> options, IEventBus eventBus, ITenantContext tenantContext) 
        : base(options)
    {
        _eventBus = eventBus;
        _tenantContext = tenantContext;
    }

    // Core entities
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserBranch> UserBranches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Set default schema based on tenant context
        var schema = GetSchemaForTenant();
        if (!string.IsNullOrEmpty(schema))
        {
            modelBuilder.HasDefaultSchema(schema);
        }
        
        // Apply configurations from all modules
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(POSDbContext).Assembly);
        
        // Configure schema-specific entities
        ConfigureSchemaSpecificEntities(modelBuilder, schema);
        
        // Global query filters for soft delete only (no tenant filtering needed with schemas)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Only apply soft delete filter since schema isolation handles tenancy
                var softDeleteFilter = GetSoftDeleteFilter(entityType.ClrType);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(softDeleteFilter);
            }
        }
    }

    private string GetSchemaForTenant()
    {
        var organizationId = _tenantContext.OrganizationId;
        if (string.IsNullOrEmpty(organizationId))
        {
            return "public"; // Default to public schema for system operations
        }
        
        return $"org_{organizationId}";
    }

    private void ConfigureSchemaSpecificEntities(ModelBuilder modelBuilder, string schema)
    {
        // System entities always go to public schema
        modelBuilder.Entity<Organization>().ToTable("organizations", "public");
        modelBuilder.Entity<Permission>().ToTable("permissions", "public");
        
        // Tenant entities go to organization-specific schema
        if (!string.IsNullOrEmpty(schema) && schema != "public")
        {
            modelBuilder.Entity<Branch>().ToTable("branches", schema);
            modelBuilder.Entity<User>().ToTable("users", schema);
            modelBuilder.Entity<Role>().ToTable("roles", schema);
            modelBuilder.Entity<UserRole>().ToTable("user_roles", schema);
            modelBuilder.Entity<RolePermission>().ToTable("role_permissions", schema);
            modelBuilder.Entity<UserBranch>().ToTable("user_branches", schema);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        var result = await base.SaveChangesAsync(cancellationToken);
        await PublishDomainEventsAsync(cancellationToken);
        return result;
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }
    }

    private async Task PublishDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEvents = ChangeTracker.Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents?.Any() == true)
            .SelectMany(x => x.Entity.DomainEvents!)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            await _eventBus.PublishAsync(domainEvent, cancellationToken);
        }

        // Clear events after publishing
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }

    private static LambdaExpression GetSoftDeleteFilter(Type entityType)
    {
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }

    private LambdaExpression GetTenantFilter(Type entityType)
    {
        // With schema-based multi-tenancy, we don't need row-level filtering
        // since each tenant has its own schema. This method can return null
        // or a simple true condition since schema isolation handles tenant separation.
        var parameter = Expression.Parameter(entityType, "e");
        var trueConstant = Expression.Constant(true);
        return Expression.Lambda(trueConstant, parameter);
    }

    private static LambdaExpression? CombineFilters(Type entityType, List<LambdaExpression> filters)
    {
        if (!filters.Any()) return null;
        if (filters.Count == 1) return filters[0];

        var parameter = Expression.Parameter(entityType, "e");
        Expression? combinedCondition = null;

        foreach (var filter in filters)
        {
            var filterBody = new ParameterReplacer(filter.Parameters[0], parameter).Visit(filter.Body);
            combinedCondition = combinedCondition == null ? filterBody : Expression.AndAlso(combinedCondition, filterBody);
        }

        return combinedCondition != null ? Expression.Lambda(combinedCondition, parameter) : null;
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;

        public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.RollbackTransactionAsync(cancellationToken);
    }
}