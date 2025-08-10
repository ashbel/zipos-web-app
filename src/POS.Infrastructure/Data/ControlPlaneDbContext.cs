using Microsoft.EntityFrameworkCore;

namespace POS.Infrastructure.Data;

public class ControlPlaneDbContext : DbContext
{
    public ControlPlaneDbContext(DbContextOptions<ControlPlaneDbContext> options) : base(options)
    {
    }

    public DbSet<TenantConnectionInfo> TenantConnections => Set<TenantConnectionInfo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TenantConnectionInfo>(b =>
        {
            b.ToTable("tenant_connection_strings");
            b.HasKey(x => x.OrganizationId);
            b.Property(x => x.OrganizationId).IsRequired();
            b.Property(x => x.ConnectionString).IsRequired();
            b.Property(x => x.Provider).HasMaxLength(100).HasDefaultValue("Npgsql");
            b.Property(x => x.CreatedAt).HasDefaultValueSql("now()");
            b.Property(x => x.UpdatedAt).HasDefaultValueSql("now()");
            b.HasIndex(x => x.OrganizationId).IsUnique();
        });
    }
}

public class TenantConnectionInfo
{
    public string OrganizationId { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Provider { get; set; } = "Npgsql";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

