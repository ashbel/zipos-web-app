using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Domain;

namespace POS.Infrastructure.Data.Configurations;

public class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("promotions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50);
        builder.Property(x => x.Type).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PercentOff).HasPrecision(18, 2);
        builder.Property(x => x.AmountOff).HasPrecision(18, 2);
        builder.Property(x => x.MinCartTotal).HasPrecision(18, 2);
        builder.Property(x => x.RequiredTier).HasMaxLength(50);
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => new { x.IsActive, x.StartDate, x.EndDate });
        builder.HasIndex(x => new { x.BranchId, x.ProductId, x.CategoryId });
    }
}


