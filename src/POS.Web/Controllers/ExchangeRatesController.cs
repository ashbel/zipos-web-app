using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.CurrencyManagement.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IExchangeRateService _exchangeRateService;

    public ExchangeRatesController(IExchangeRateService exchangeRateService)
    {
        _exchangeRateService = exchangeRateService;
    }

    [HttpGet("current")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> GetCurrentExchangeRates()
    {
        var rates = await _exchangeRateService.GetCurrentExchangeRatesAsync();
        return Ok(rates);
    }

    [HttpGet("{fromCurrency}/{toCurrency}")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> GetExchangeRate(string fromCurrency, string toCurrency, [FromQuery] DateTime? date = null)
    {
        var rate = await _exchangeRateService.GetExchangeRateAsync(fromCurrency, toCurrency, date);
        if (rate == null)
        {
            return NotFound($"No exchange rate found for {fromCurrency} to {toCurrency}");
        }
        return Ok(rate);
    }

    [HttpGet("{fromCurrency}/{toCurrency}/history")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> GetExchangeRateHistory(
        string fromCurrency, 
        string toCurrency, 
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        var dateRange = new DateRange(startDate, endDate);
        var history = await _exchangeRateService.GetExchangeRateHistoryAsync(fromCurrency, toCurrency, dateRange);
        return Ok(history);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> SetExchangeRate([FromBody] SetExchangeRateRequest request)
    {
        try
        {
            var exchangeRate = await _exchangeRateService.SetExchangeRateAsync(
                request.FromCurrency, 
                request.ToCurrency, 
                request.Rate, 
                request.Source, 
                request.SourceReference);
            
            return CreatedAtAction(
                nameof(GetExchangeRate), 
                new { fromCurrency = exchangeRate.FromCurrencyCode, toCurrency = exchangeRate.ToCurrencyCode }, 
                exchangeRate);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("convert")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> ConvertAmount([FromBody] ConvertAmountRequest request)
    {
        try
        {
            var convertedAmount = await _exchangeRateService.ConvertAmountAsync(
                request.Amount, 
                request.FromCurrency, 
                request.ToCurrency, 
                request.Date);
            
            return Ok(new ConvertAmountResponse(
                request.Amount,
                request.FromCurrency,
                convertedAmount,
                request.ToCurrency,
                request.Date ?? DateTime.UtcNow
            ));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("update-from-provider")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> UpdateExchangeRatesFromProvider()
    {
        var success = await _exchangeRateService.UpdateExchangeRatesFromProviderAsync();
        if (!success)
        {
            return BadRequest("No external exchange rate provider configured");
        }
        return Ok(new { message = "Exchange rates updated successfully" });
    }
}

public record SetExchangeRateRequest(
    string FromCurrency,
    string ToCurrency,
    decimal Rate,
    POS.Shared.Domain.ExchangeRateSource Source = POS.Shared.Domain.ExchangeRateSource.Manual,
    string? SourceReference = null
);

public record ConvertAmountRequest(
    decimal Amount,
    string FromCurrency,
    string ToCurrency,
    DateTime? Date = null
);

public record ConvertAmountResponse(
    decimal OriginalAmount,
    string FromCurrency,
    decimal ConvertedAmount,
    string ToCurrency,
    DateTime ConversionDate
);