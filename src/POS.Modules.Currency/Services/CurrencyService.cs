using Microsoft.EntityFrameworkCore;
using POS.Infrastructure.Data;
using POS.Shared.Domain;
using POS.Shared.Domain.Exceptions;

namespace POS.Modules.CurrencyManagement.Services;

public class CurrencyService : ICurrencyService
{
    private readonly POSDbContext _context;

    public CurrencyService(POSDbContext context)
    {
        _context = context;
    }

    public async Task<Currency> CreateCurrencyAsync(CreateCurrencyRequest request)
    {
        // Check if currency code already exists
        var existingCurrency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Code == request.Code.ToUpper());

        if (existingCurrency != null)
        {
            throw new BusinessRuleException("DUPLICATE_CURRENCY_CODE", $"Currency with code '{request.Code}' already exists");
        }

        // If this is set as base currency, ensure no other base currency exists
        if (request.IsBaseCurrency)
        {
            var existingBaseCurrency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.IsBaseCurrency);

            if (existingBaseCurrency != null)
            {
                throw new BusinessRuleException("MULTIPLE_BASE_CURRENCIES", "A base currency already exists. Only one base currency is allowed.");
            }
        }

        var currency = new Currency
        {
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Symbol = request.Symbol,
            DecimalPlaces = request.DecimalPlaces,
            IsActive = request.IsActive,
            IsBaseCurrency = request.IsBaseCurrency
        };

        _context.Currencies.Add(currency);
        await _context.SaveChangesAsync();

        return currency;
    }

    public async Task<Currency> UpdateCurrencyAsync(string currencyId, UpdateCurrencyRequest request)
    {
        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == currencyId);

        if (currency == null)
        {
            throw new BusinessRuleException("CURRENCY_NOT_FOUND", $"Currency with ID '{currencyId}' not found");
        }

        if (request.Name != null)
            currency.Name = request.Name;

        if (request.Symbol != null)
            currency.Symbol = request.Symbol;

        if (request.DecimalPlaces.HasValue)
            currency.DecimalPlaces = request.DecimalPlaces.Value;

        if (request.IsActive.HasValue)
            currency.IsActive = request.IsActive.Value;

        currency.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return currency;
    }

    public async Task<IEnumerable<Currency>> GetActiveCurrenciesAsync()
    {
        return await _context.Currencies
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.Code)
            .ToListAsync();
    }

    public async Task<Currency?> GetBaseCurrencyAsync()
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(c => c.IsBaseCurrency && c.IsActive && !c.IsDeleted);
    }

    public async Task<Currency?> GetCurrencyByCodeAsync(string currencyCode)
    {
        return await _context.Currencies
            .FirstOrDefaultAsync(c => c.Code == currencyCode.ToUpper() && !c.IsDeleted);
    }

    public async Task<bool> SetBaseCurrencyAsync(string currencyCode)
    {
        var currency = await GetCurrencyByCodeAsync(currencyCode);
        if (currency == null || !currency.IsActive)
        {
            return false;
        }

        // Remove base currency flag from existing base currency
        var existingBaseCurrency = await GetBaseCurrencyAsync();
        if (existingBaseCurrency != null)
        {
            existingBaseCurrency.IsBaseCurrency = false;
            existingBaseCurrency.UpdatedAt = DateTime.UtcNow;
        }

        // Set new base currency
        currency.IsBaseCurrency = true;
        currency.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateCurrencyAsync(string currencyId)
    {
        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == currencyId);

        if (currency == null)
        {
            return false;
        }

        if (currency.IsBaseCurrency)
        {
            throw new BusinessRuleException("DEACTIVATE_BASE_CURRENCY", "Cannot deactivate the base currency");
        }

        currency.IsActive = false;
        currency.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivateCurrencyAsync(string currencyId)
    {
        var currency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Id == currencyId);

        if (currency == null)
        {
            return false;
        }

        currency.IsActive = true;
        currency.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}