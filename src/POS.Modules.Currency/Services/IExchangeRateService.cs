using POS.Shared.Domain;

namespace POS.Modules.CurrencyManagement.Services;

public interface IExchangeRateService
{
    Task<ExchangeRate> SetExchangeRateAsync(string fromCurrency, string toCurrency, decimal rate, ExchangeRateSource source = ExchangeRateSource.Manual, string? sourceReference = null);
    Task<ExchangeRate?> GetExchangeRateAsync(string fromCurrency, string toCurrency, DateTime? date = null);
    Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? date = null);
    Task<bool> UpdateExchangeRatesFromProviderAsync();
    Task<IEnumerable<ExchangeRate>> GetExchangeRateHistoryAsync(string fromCurrency, string toCurrency, DateRange dateRange);
    Task<IEnumerable<ExchangeRate>> GetCurrentExchangeRatesAsync();
}

public record DateRange(DateTime StartDate, DateTime EndDate);