using Microsoft.EntityFrameworkCore;
using Moq;
using POS.Infrastructure.Data;
using POS.Modules.CurrencyManagement.Services;
using POS.Shared.Domain;
using POS.Shared.Domain.Exceptions;
using POS.Shared.Infrastructure;
using Xunit;

namespace POS.Tests.Currency;

public class ExchangeRateServiceTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly ExchangeRateService _exchangeRateService;
    private readonly CurrencyService _currencyService;

    public ExchangeRateServiceTests()
    {
        var options = new DbContextOptionsBuilder<POSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockEventBus = new Mock<IEventBus>();
        var mockTenantContext = new Mock<ITenantContext>();
        
        _context = new POSDbContext(options, mockEventBus.Object, mockTenantContext.Object);
        _currencyService = new CurrencyService(_context);
        _exchangeRateService = new ExchangeRateService(_context, _currencyService);
    }

    [Fact]
    public async Task SetExchangeRateAsync_ValidCurrencies_CreatesExchangeRate()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));

        // Act
        var result = await _exchangeRateService.SetExchangeRateAsync("USD", "EUR", 0.85m);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD", result.FromCurrencyCode);
        Assert.Equal("EUR", result.ToCurrencyCode);
        Assert.Equal(0.85m, result.Rate);
        Assert.Equal(ExchangeRateSource.Manual, result.Source);
    }

    [Fact]
    public async Task SetExchangeRateAsync_InvalidFromCurrency_ThrowsException()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => 
            _exchangeRateService.SetExchangeRateAsync("INVALID", "EUR", 0.85m));
    }

    [Fact]
    public async Task SetExchangeRateAsync_InvalidToCurrency_ThrowsException()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => 
            _exchangeRateService.SetExchangeRateAsync("USD", "INVALID", 0.85m));
    }

    [Fact]
    public async Task SetExchangeRateAsync_ZeroRate_ThrowsException()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => 
            _exchangeRateService.SetExchangeRateAsync("USD", "EUR", 0m));
    }

    [Fact]
    public async Task GetExchangeRateAsync_SameCurrency_ReturnsRateOfOne()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));

        // Act
        var result = await _exchangeRateService.GetExchangeRateAsync("USD", "USD");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD", result.FromCurrencyCode);
        Assert.Equal("USD", result.ToCurrencyCode);
        Assert.Equal(1.0m, result.Rate);
        Assert.Equal(ExchangeRateSource.System, result.Source);
    }

    [Fact]
    public async Task GetExchangeRateAsync_DirectRate_ReturnsDirectRate()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));
        await _exchangeRateService.SetExchangeRateAsync("USD", "EUR", 0.85m);

        // Act
        var result = await _exchangeRateService.GetExchangeRateAsync("USD", "EUR");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD", result.FromCurrencyCode);
        Assert.Equal("EUR", result.ToCurrencyCode);
        Assert.Equal(0.85m, result.Rate);
    }

    [Fact]
    public async Task GetExchangeRateAsync_InverseRate_ReturnsInverseRate()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));
        await _exchangeRateService.SetExchangeRateAsync("EUR", "USD", 1.18m);

        // Act
        var result = await _exchangeRateService.GetExchangeRateAsync("USD", "EUR");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD", result.FromCurrencyCode);
        Assert.Equal("EUR", result.ToCurrencyCode);
        Assert.Equal(1m / 1.18m, result.Rate, 8); // Check with precision
    }

    [Fact]
    public async Task GetExchangeRateAsync_NoRateFound_ReturnsNull()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));

        // Act
        var result = await _exchangeRateService.GetExchangeRateAsync("USD", "EUR");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ConvertAmountAsync_ValidConversion_ReturnsConvertedAmount()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));
        await _exchangeRateService.SetExchangeRateAsync("USD", "EUR", 0.85m);

        // Act
        var result = await _exchangeRateService.ConvertAmountAsync(100m, "USD", "EUR");

        // Assert
        Assert.Equal(85m, result);
    }

    [Fact]
    public async Task ConvertAmountAsync_ZeroAmount_ReturnsZero()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));

        // Act
        var result = await _exchangeRateService.ConvertAmountAsync(0m, "USD", "EUR");

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task ConvertAmountAsync_NoExchangeRate_ThrowsException()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => 
            _exchangeRateService.ConvertAmountAsync(100m, "USD", "EUR"));
    }

    [Fact]
    public async Task GetExchangeRateHistoryAsync_ReturnsHistoricalRates()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));
        
        var startDate = DateTime.UtcNow.AddDays(-10);
        
        await _exchangeRateService.SetExchangeRateAsync("USD", "EUR", 0.85m);
        await Task.Delay(10); // Ensure different timestamps
        await _exchangeRateService.SetExchangeRateAsync("USD", "EUR", 0.86m);
        
        var endDate = DateTime.UtcNow.AddDays(1); // Set end date after creating rates

        // Act
        var result = await _exchangeRateService.GetExchangeRateHistoryAsync("USD", "EUR", new DateRange(startDate, endDate));

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, r => Assert.Equal("USD", r.FromCurrencyCode));
        Assert.All(result, r => Assert.Equal("EUR", r.ToCurrencyCode));
    }

    [Fact]
    public async Task GetCurrentExchangeRatesAsync_ReturnsLatestRates()
    {
        // Arrange
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false));
        await _currencyService.CreateCurrencyAsync(new CreateCurrencyRequest("GBP", "British Pound", "£", 2, true, false));
        
        await _exchangeRateService.SetExchangeRateAsync("USD", "EUR", 0.85m);
        await _exchangeRateService.SetExchangeRateAsync("USD", "GBP", 0.75m);
        await _exchangeRateService.SetExchangeRateAsync("EUR", "GBP", 0.88m);

        // Act
        var result = await _exchangeRateService.GetCurrentExchangeRatesAsync();

        // Assert
        Assert.Equal(3, result.Count());
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}