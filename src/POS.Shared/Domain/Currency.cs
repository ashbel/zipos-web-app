namespace POS.Shared.Domain;

public class Currency : TenantEntity
{
    public string Code { get; set; } = string.Empty; // ISO 4217 currency code (USD, EUR, GBP, etc.)
    public string Name { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPlaces { get; set; } = 2; // Number of decimal places for the currency
    public bool IsActive { get; set; } = true;
    public bool IsBaseCurrency { get; set; } = false;
}

public class ExchangeRate : TenantEntity
{
    public string FromCurrencyCode { get; set; } = string.Empty;
    public string ToCurrencyCode { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public ExchangeRateSource Source { get; set; } = ExchangeRateSource.Manual;
    public string? SourceReference { get; set; } // Reference to external rate source
}

public class ProductPrice : TenantEntity
{
    public string ProductId { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum ExchangeRateSource
{
    Manual,
    ExternalAPI,
    BankFeed,
    System
}