using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using POS.Shared.Domain;

namespace POS.Infrastructure.Data.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("currencies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Symbol).HasMaxLength(10).IsRequired();
        builder.Property(x => x.DecimalPlaces).HasDefaultValue(2);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.IsBaseCurrency).HasDefaultValue(false);
        
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.IsBaseCurrency);
    }
}

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FromCurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ToCurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Rate).HasPrecision(18, 8).IsRequired();
        builder.Property(x => x.EffectiveDate).HasDefaultValueSql("now()");
        builder.Property(x => x.Source).HasConversion<string>();
        builder.Property(x => x.SourceReference).HasMaxLength(200);
        
        builder.HasIndex(x => new { x.FromCurrencyCode, x.ToCurrencyCode, x.EffectiveDate });
    }
}

public class ProductPriceConfiguration : IEntityTypeConfiguration<ProductPrice>
{
    public void Configure(EntityTypeBuilder<ProductPrice> builder)
    {
        builder.ToTable("product_prices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.EffectiveDate).HasDefaultValueSql("now()");
        builder.Property(x => x.ExpiryDate);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        
        builder.HasIndex(x => new { x.ProductId, x.CurrencyCode, x.EffectiveDate });
        builder.HasIndex(x => new { x.ProductId, x.CurrencyCode, x.IsActive });
    }
}