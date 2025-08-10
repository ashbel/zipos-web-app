using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Domain;

namespace POS.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SKU).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Barcode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.BasePrice).HasPrecision(18, 2);
        builder.Property(x => x.Cost).HasPrecision(18, 2);
        builder.Property(x => x.CategoryId).HasMaxLength(50);
        builder.Property(x => x.Attributes);
        builder.Property(x => x.ImageUrl).HasMaxLength(1000);
        builder.HasIndex(x => x.SKU).IsUnique();
        builder.HasIndex(x => x.Barcode).IsUnique();
        builder.HasIndex(x => x.Name);
    }
}

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.CurrentStock).HasPrecision(18, 3);
        builder.Property(x => x.ReorderLevel).HasPrecision(18, 3);
        builder.Property(x => x.AverageCost).HasPrecision(18, 4);
        builder.Property(x => x.LastPurchasePrice).HasPrecision(18, 4);
        builder.Property(x => x.LastUpdated).HasDefaultValueSql("now()");
        builder.HasIndex(x => new { x.ProductId, x.BranchId }).IsUnique();
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.QuantityDelta).HasPrecision(18, 3);
        builder.Property(x => x.Reason).HasMaxLength(200);
        builder.Property(x => x.ReferenceId).HasMaxLength(100);
        builder.Property(x => x.PerformedBy).HasMaxLength(100);
        builder.Property(x => x.PerformedAt).HasDefaultValueSql("now()");
        builder.HasIndex(x => new { x.ProductId, x.BranchId, x.PerformedAt });
    }
}

public class StockAlertConfiguration : IEntityTypeConfiguration<StockAlert>
{
    public void Configure(EntityTypeBuilder<StockAlert> builder)
    {
        builder.ToTable("stock_alerts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.CurrentStock).HasPrecision(18, 3);
        builder.Property(x => x.ReorderLevel).HasPrecision(18, 3);
        builder.Property(x => x.IsAcknowledged).HasDefaultValue(false);
        builder.Property(x => x.AcknowledgedAt);
        builder.HasIndex(x => new { x.ProductId, x.BranchId }).IsUnique();
    }
}

public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("stock_adjustments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.QuantityDelta).HasPrecision(18, 3);
        builder.Property(x => x.Reason).HasMaxLength(200);
        builder.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Pending");
        builder.Property(x => x.RequestedBy).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(100);
        builder.Property(x => x.RequestedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.ApprovedAt);
        builder.HasIndex(x => new { x.ProductId, x.BranchId, x.Status });
    }
}

public class StocktakeSessionConfiguration : IEntityTypeConfiguration<StocktakeSession>
{
    public void Configure(EntityTypeBuilder<StocktakeSession> builder)
    {
        builder.ToTable("stocktake_sessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).HasDefaultValue("Open");
        builder.Property(x => x.StartedBy).HasMaxLength(100);
        builder.Property(x => x.FinalizedBy).HasMaxLength(100);
        builder.Property(x => x.StartedAt).HasDefaultValueSql("now()");
        builder.Property(x => x.FinalizedAt);
        builder.HasIndex(x => new { x.BranchId, x.Status });
    }
}

public class StocktakeLineConfiguration : IEntityTypeConfiguration<StocktakeLine>
{
    public void Configure(EntityTypeBuilder<StocktakeLine> builder)
    {
        builder.ToTable("stocktake_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SessionId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.ExpectedQty).HasPrecision(18, 3);
        builder.Property(x => x.CountedQty).HasPrecision(18, 3);
        builder.Property(x => x.VarianceQty).HasPrecision(18, 3);
        builder.HasIndex(x => new { x.SessionId, x.ProductId }).IsUnique();
    }
}

