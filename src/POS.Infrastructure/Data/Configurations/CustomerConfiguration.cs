using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Domain;

namespace POS.Infrastructure.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(255);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.TaxId).HasMaxLength(50);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.LoyaltyTier).HasMaxLength(50);
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.Phone);
    }
}

