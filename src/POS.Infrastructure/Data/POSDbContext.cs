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
        // Database-per-tenant: no default schema switching

        // Apply configurations from all modules
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(POSDbContext).Assembly);
        
        // Map system tables (control-plane) when no tenant is set
        ConfigureEntityTables(modelBuilder);
        
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

    private void ConfigureEntityTables(ModelBuilder modelBuilder)
    {
        // Database-per-tenant: map to unqualified table names; each tenant DB has its own tables
        modelBuilder.Entity<Organization>().ToTable("organizations");
        modelBuilder.Entity<Permission>().ToTable("permissions");
        modelBuilder.Entity<Branch>().ToTable("branches");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Role>().ToTable("roles");
        modelBuilder.Entity<UserRole>().ToTable("user_roles");
        modelBuilder.Entity<RolePermission>().ToTable("role_permissions");
        modelBuilder.Entity<UserBranch>().ToTable("user_branches");
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
        // Database-per-tenant: no row-level tenant filtering required
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