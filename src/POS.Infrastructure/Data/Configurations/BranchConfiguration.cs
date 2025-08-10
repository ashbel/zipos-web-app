using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Domain;

namespace POS.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branches");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(x => x.ContactPhone)
            .HasMaxLength(50);
            
        builder.Property(x => x.ContactEmail)
            .HasMaxLength(255);
            
        builder.Property(x => x.TimeZone)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3);
            
        builder.Property(x => x.TaxRate)
            .HasPrecision(5, 4);
            
        builder.Property(x => x.Settings)
            .HasColumnType("jsonb");

        // Address value object
        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.State).HasMaxLength(100);
            address.Property(a => a.PostalCode).HasMaxLength(20);
            address.Property(a => a.Country).HasMaxLength(100);
        });

        // Indexes (no OrganizationId needed with schema isolation)
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name);
    }
}