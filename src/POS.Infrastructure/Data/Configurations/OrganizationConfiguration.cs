using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Domain;
using POS.Shared.Domain.ValueObjects;
using System.Text.Json;

namespace POS.Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(x => x.SubscriptionPlan)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(x => x.ContactEmail)
            .HasMaxLength(255);
            
        builder.Property(x => x.ContactPhone)
            .HasMaxLength(50);

        // Address value object
        builder.OwnsOne(x => x.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.State).HasMaxLength(100);
            address.Property(a => a.PostalCode).HasMaxLength(20);
            address.Property(a => a.Country).HasMaxLength(100);
        });

        // Indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.ContactEmail);
        
        // Note: No relationships to Branches and Users since they exist in separate tenant schemas
        // Schema-based multi-tenancy provides natural isolation without foreign key relationships
    }
}