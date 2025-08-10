using Microsoft.EntityFrameworkCore;
using POS.Shared.Domain;
using Xunit;

namespace POS.Tests.Infrastructure;

public class TestEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    
    public DbSet<TestEntity> TestEntities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
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
}

public class RepositoryTests
{
    private TestDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var entity = new TestEntity { Name = "Test Entity" };

        // Act
        await context.TestEntities.AddAsync(entity);
        await context.SaveChangesAsync();

        // Assert
        var savedEntity = await context.TestEntities.FindAsync(entity.Id);
        Assert.NotNull(savedEntity);
        Assert.Equal("Test Entity", savedEntity.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnEntity_WhenEntityExists()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var entity = new TestEntity { Name = "Test Entity" };
        
        await context.TestEntities.AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        var result = await context.TestEntities.FindAsync(entity.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
        Assert.Equal("Test Entity", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenEntityDoesNotExist()
    {
        // Arrange
        using var context = GetInMemoryContext();

        // Act
        var result = await context.TestEntities.FindAsync("non-existent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntity()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var entity = new TestEntity { Name = "Original Name" };
        
        await context.TestEntities.AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        entity.Name = "Updated Name";
        context.TestEntities.Update(entity);
        await context.SaveChangesAsync();

        // Assert
        var updatedEntity = await context.TestEntities.FindAsync(entity.Id);
        Assert.NotNull(updatedEntity);
        Assert.Equal("Updated Name", updatedEntity.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteEntity()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var entity = new TestEntity { Name = "To Be Deleted" };
        
        await context.TestEntities.AddAsync(entity);
        await context.SaveChangesAsync();

        // Act
        context.TestEntities.Remove(entity);
        await context.SaveChangesAsync();

        // Assert
        // Check that the entity is marked as deleted but still exists in the database
        var deletedEntity = await context.TestEntities.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == entity.Id);
        Assert.NotNull(deletedEntity);
        Assert.True(deletedEntity.IsDeleted);
        Assert.NotNull(deletedEntity.DeletedAt);
        
        // Check that the entity is not returned by normal queries due to query filter
        var filteredEntity = await context.TestEntities.FirstOrDefaultAsync(e => e.Id == entity.Id);
        Assert.Null(filteredEntity);
    }
}