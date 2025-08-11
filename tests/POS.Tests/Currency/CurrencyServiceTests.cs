using Microsoft.EntityFrameworkCore;
using Moq;
using POS.Infrastructure.Data;
using POS.Modules.CurrencyManagement.Services;
using POS.Shared.Domain;
using POS.Shared.Domain.Exceptions;
using POS.Shared.Infrastructure;
using Xunit;

namespace POS.Tests.Currency;

public class CurrencyServiceTests : IDisposable
{
    private readonly POSDbContext _context;
    private readonly CurrencyService _currencyService;

    public CurrencyServiceTests()
    {
        var options = new DbContextOptionsBuilder<POSDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockEventBus = new Mock<IEventBus>();
        var mockTenantContext = new Mock<ITenantContext>();
        
        _context = new POSDbContext(options, mockEventBus.Object, mockTenantContext.Object);
        _currencyService = new CurrencyService(_context);
    }

    [Fact]
    public async Task CreateCurrencyAsync_ValidRequest_CreatesCurrency()
    {
        // Arrange
        var request = new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true);

        // Act
        var result = await _currencyService.CreateCurrencyAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD", result.Code);
        Assert.Equal("US Dollar", result.Name);
        Assert.Equal("$", result.Symbol);
        Assert.Equal(2, result.DecimalPlaces);
        Assert.True(result.IsActive);
        Assert.True(result.IsBaseCurrency);
    }

    [Fact]
    public async Task CreateCurrencyAsync_DuplicateCode_ThrowsException()
    {
        // Arrange
        var request1 = new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, false);
        var request2 = new CreateCurrencyRequest("usd", "US Dollar 2", "$", 2, true, false);

        await _currencyService.CreateCurrencyAsync(request1);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _currencyService.CreateCurrencyAsync(request2));
    }

    [Fact]
    public async Task CreateCurrencyAsync_MultipleBaseCurrencies_ThrowsException()
    {
        // Arrange
        var request1 = new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true);
        var request2 = new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, true);

        await _currencyService.CreateCurrencyAsync(request1);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _currencyService.CreateCurrencyAsync(request2));
    }

    [Fact]
    public async Task GetBaseCurrencyAsync_BaseCurrencyExists_ReturnsBaseCurrency()
    {
        // Arrange
        var request = new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true);
        await _currencyService.CreateCurrencyAsync(request);

        // Act
        var result = await _currencyService.GetBaseCurrencyAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("USD", result.Code);
        Assert.True(result.IsBaseCurrency);
    }

    [Fact]
    public async Task GetBaseCurrencyAsync_NoBaseCurrency_ReturnsNull()
    {
        // Act
        var result = await _currencyService.GetBaseCurrencyAsync();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetBaseCurrencyAsync_ValidCurrency_SetsBaseCurrency()
    {
        // Arrange
        var request1 = new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true);
        var request2 = new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false);
        
        await _currencyService.CreateCurrencyAsync(request1);
        await _currencyService.CreateCurrencyAsync(request2);

        // Act
        var result = await _currencyService.SetBaseCurrencyAsync("EUR");

        // Assert
        Assert.True(result);
        
        var baseCurrency = await _currencyService.GetBaseCurrencyAsync();
        Assert.NotNull(baseCurrency);
        Assert.Equal("EUR", baseCurrency.Code);
        
        var usdCurrency = await _currencyService.GetCurrencyByCodeAsync("USD");
        Assert.NotNull(usdCurrency);
        Assert.False(usdCurrency.IsBaseCurrency);
    }

    [Fact]
    public async Task SetBaseCurrencyAsync_InvalidCurrency_ReturnsFalse()
    {
        // Act
        var result = await _currencyService.SetBaseCurrencyAsync("INVALID");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeactivateCurrencyAsync_BaseCurrency_ThrowsException()
    {
        // Arrange
        var request = new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, true);
        var currency = await _currencyService.CreateCurrencyAsync(request);

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleException>(() => _currencyService.DeactivateCurrencyAsync(currency.Id));
    }

    [Fact]
    public async Task DeactivateCurrencyAsync_NonBaseCurrency_DeactivatesCurrency()
    {
        // Arrange
        var request = new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false);
        var currency = await _currencyService.CreateCurrencyAsync(request);

        // Act
        var result = await _currencyService.DeactivateCurrencyAsync(currency.Id);

        // Assert
        Assert.True(result);
        
        var updatedCurrency = await _currencyService.GetCurrencyByCodeAsync("EUR");
        Assert.NotNull(updatedCurrency);
        Assert.False(updatedCurrency.IsActive);
    }

    [Fact]
    public async Task GetActiveCurrenciesAsync_ReturnsOnlyActiveCurrencies()
    {
        // Arrange
        var request1 = new CreateCurrencyRequest("USD", "US Dollar", "$", 2, true, false);
        var request2 = new CreateCurrencyRequest("EUR", "Euro", "€", 2, true, false);
        var request3 = new CreateCurrencyRequest("GBP", "British Pound", "£", 2, false, false);

        await _currencyService.CreateCurrencyAsync(request1);
        await _currencyService.CreateCurrencyAsync(request2);
        await _currencyService.CreateCurrencyAsync(request3);

        // Act
        var result = await _currencyService.GetActiveCurrenciesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, c => Assert.True(c.IsActive));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}