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

public class CustomerLoyaltyConfiguration : IEntityTypeConfiguration<CustomerLoyalty>
{
    public void Configure(EntityTypeBuilder<CustomerLoyalty> builder)
    {
        builder.ToTable("customer_loyalty");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.Points).HasDefaultValue(0);
        builder.Property(x => x.Tier).HasMaxLength(50).HasDefaultValue("Basic");
        builder.Property(x => x.LastUpdated).HasDefaultValueSql("now()");
        builder.HasIndex(x => x.CustomerId).IsUnique();
    }
}

public class LoyaltyTierDefinitionConfiguration : IEntityTypeConfiguration<LoyaltyTierDefinition>
{
    public void Configure(EntityTypeBuilder<LoyaltyTierDefinition> builder)
    {
        builder.ToTable("loyalty_tiers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.MinPoints).IsRequired();
        builder.Property(x => x.MaxPoints);
        builder.Property(x => x.DiscountPercent).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(x => x.Priority).HasDefaultValue(0);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}

public class CustomerCreditConfiguration : IEntityTypeConfiguration<CustomerCredit>
{
    public void Configure(EntityTypeBuilder<CustomerCredit> builder)
    {
        builder.ToTable("customer_credit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.CreditLimit).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(x => x.OutstandingBalance).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Active");
        builder.Property(x => x.LastUpdated).HasDefaultValueSql("now()");
        builder.HasIndex(x => x.CustomerId).IsUnique();
    }
}

public class CustomerCreditTransactionConfiguration : IEntityTypeConfiguration<CustomerCreditTransaction>
{
    public void Configure(EntityTypeBuilder<CustomerCreditTransaction> builder)
    {
        builder.ToTable("customer_credit_txns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.OccurredAt).HasDefaultValueSql("now()");
        builder.Property(x => x.Type).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.Reference).HasMaxLength(100);
        builder.Property(x => x.Note).HasMaxLength(500);
        builder.Property(x => x.SaleId);
        builder.HasIndex(x => new { x.CustomerId, x.OccurredAt });
    }
}

