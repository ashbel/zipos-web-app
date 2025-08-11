using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Domain.Exceptions;

namespace POS.Modules.CurrencyManagement.Services;

public class ExchangeRateService : IExchangeRateService
{
    private readonly POSDbContext _context;
    private readonly ICurrencyService _currencyService;

    public ExchangeRateService(POSDbContext context, ICurrencyService currencyService)
    {
        _context = context;
        _currencyService = currencyService;
    }

    public async Task<ExchangeRate> SetExchangeRateAsync(string fromCurrency, string toCurrency, decimal rate, ExchangeRateSource source = ExchangeRateSource.Manual, string? sourceReference = null)
    {
        // Validate currencies exist
        var fromCurrencyEntity = await _currencyService.GetCurrencyByCodeAsync(fromCurrency);
        var toCurrencyEntity = await _currencyService.GetCurrencyByCodeAsync(toCurrency);

        if (fromCurrencyEntity == null)
        {
            throw new BusinessRuleException("CURRENCY_NOT_FOUND", $"Currency '{fromCurrency}' not found");
        }

        if (toCurrencyEntity == null)
        {
            throw new BusinessRuleException("CURRENCY_NOT_FOUND", $"Currency '{toCurrency}' not found");
        }

        if (rate <= 0)
        {
            throw new BusinessRuleException("INVALID_EXCHANGE_RATE", "Exchange rate must be greater than zero");
        }

        var exchangeRate = new ExchangeRate
        {
            FromCurrencyCode = fromCurrency.ToUpper(),
            ToCurrencyCode = toCurrency.ToUpper(),
            Rate = rate,
            EffectiveDate = DateTime.UtcNow,
            Source = source,
            SourceReference = sourceReference
        };

        _context.ExchangeRates.Add(exchangeRate);
        await _context.SaveChangesAsync();

        return exchangeRate;
    }

    public async Task<ExchangeRate?> GetExchangeRateAsync(string fromCurrency, string toCurrency, DateTime? date = null)
    {
        var effectiveDate = date ?? DateTime.UtcNow;

        // If same currency, return rate of 1
        if (fromCurrency.Equals(toCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return new ExchangeRate
            {
                FromCurrencyCode = fromCurrency.ToUpper(),
                ToCurrencyCode = toCurrency.ToUpper(),
                Rate = 1.0m,
                EffectiveDate = effectiveDate,
                Source = ExchangeRateSource.System
            };
        }

        // Get the most recent exchange rate before or on the effective date
        var exchangeRate = await _context.ExchangeRates
            .Where(er => er.FromCurrencyCode == fromCurrency.ToUpper() 
                        && er.ToCurrencyCode == toCurrency.ToUpper()
                        && er.EffectiveDate <= effectiveDate
                        && !er.IsDeleted)
            .OrderByDescending(er => er.EffectiveDate)
            .FirstOrDefaultAsync();

        // If no direct rate found, try to find inverse rate
        if (exchangeRate == null)
        {
            var inverseRate = await _context.ExchangeRates
                .Where(er => er.FromCurrencyCode == toCurrency.ToUpper() 
                            && er.ToCurrencyCode == fromCurrency.ToUpper()
                            && er.EffectiveDate <= effectiveDate
                            && !er.IsDeleted)
                .OrderByDescending(er => er.EffectiveDate)
                .FirstOrDefaultAsync();

            if (inverseRate != null)
            {
                return new ExchangeRate
                {
                    FromCurrencyCode = fromCurrency.ToUpper(),
                    ToCurrencyCode = toCurrency.ToUpper(),
                    Rate = 1 / inverseRate.Rate,
                    EffectiveDate = inverseRate.EffectiveDate,
                    Source = inverseRate.Source
                };
            }
        }

        return exchangeRate;
    }

    public async Task<decimal> ConvertAmountAsync(decimal amount, string fromCurrency, string toCurrency, DateTime? date = null)
    {
        if (amount == 0)
        {
            return 0;
        }

        var exchangeRate = await GetExchangeRateAsync(fromCurrency, toCurrency, date);
        
        if (exchangeRate == null)
        {
            throw new BusinessRuleException("EXCHANGE_RATE_NOT_FOUND", $"No exchange rate found for {fromCurrency} to {toCurrency}");
        }

        return amount * exchangeRate.Rate;
    }

    public async Task<bool> UpdateExchangeRatesFromProviderAsync()
    {
        // This is a placeholder for external API integration
        // In a real implementation, this would call an external service like:
        // - Fixer.io
        // - CurrencyLayer
        // - Open Exchange Rates
        // - Central bank APIs
        
        // For now, return false to indicate no external provider is configured
        await Task.CompletedTask;
        return false;
    }

    public async Task<IEnumerable<ExchangeRate>> GetExchangeRateHistoryAsync(string fromCurrency, string toCurrency, DateRange dateRange)
    {
        return await _context.ExchangeRates
            .Where(er => er.FromCurrencyCode == fromCurrency.ToUpper()
                        && er.ToCurrencyCode == toCurrency.ToUpper()
                        && er.EffectiveDate >= dateRange.StartDate
                        && er.EffectiveDate <= dateRange.EndDate
                        && !er.IsDeleted)
            .OrderByDescending(er => er.EffectiveDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ExchangeRate>> GetCurrentExchangeRatesAsync()
    {
        // Get the most recent exchange rate for each currency pair
        var latestRates = await _context.ExchangeRates
            .Where(er => !er.IsDeleted)
            .GroupBy(er => new { er.FromCurrencyCode, er.ToCurrencyCode })
            .Select(g => g.OrderByDescending(er => er.EffectiveDate).First())
            .ToListAsync();

        return latestRates;
    }
}