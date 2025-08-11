using POS.Shared.Domain;

namespace POS.Modules.CurrencyManagement.Services;

public interface ICurrencyService
{
    Task<Currency> CreateCurrencyAsync(CreateCurrencyRequest request);
    Task<Currency> UpdateCurrencyAsync(string currencyId, UpdateCurrencyRequest request);
    Task<IEnumerable<Currency>> GetActiveCurrenciesAsync();
    Task<Currency?> GetBaseCurrencyAsync();
    Task<Currency?> GetCurrencyByCodeAsync(string currencyCode);
    Task<bool> SetBaseCurrencyAsync(string currencyCode);
    Task<bool> DeactivateCurrencyAsync(string currencyId);
    Task<bool> ActivateCurrencyAsync(string currencyId);
}

public record CreateCurrencyRequest(
    string Code,
    string Name,
    string Symbol,
    int DecimalPlaces = 2,
    bool IsActive = true,
    bool IsBaseCurrency = false
);

public record UpdateCurrencyRequest(
    string? Name = null,
    string? Symbol = null,
    int? DecimalPlaces = null,
    bool? IsActive = null
);