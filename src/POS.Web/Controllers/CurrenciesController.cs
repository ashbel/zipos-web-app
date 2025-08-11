using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.CurrencyManagement.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    [HttpGet]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> GetActiveCurrencies()
    {
        var currencies = await _currencyService.GetActiveCurrenciesAsync();
        return Ok(currencies);
    }

    [HttpGet("base")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> GetBaseCurrency()
    {
        var baseCurrency = await _currencyService.GetBaseCurrencyAsync();
        if (baseCurrency == null)
        {
            return NotFound("No base currency configured");
        }
        return Ok(baseCurrency);
    }

    [HttpGet("{code}")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> GetCurrencyByCode(string code)
    {
        var currency = await _currencyService.GetCurrencyByCodeAsync(code);
        if (currency == null)
        {
            return NotFound($"Currency with code '{code}' not found");
        }
        return Ok(currency);
    }

    [HttpPost]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> CreateCurrency([FromBody] CreateCurrencyRequest request)
    {
        try
        {
            var currency = await _currencyService.CreateCurrencyAsync(request);
            return CreatedAtAction(nameof(GetCurrencyByCode), new { code = currency.Code }, currency);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> UpdateCurrency(string id, [FromBody] UpdateCurrencyRequest request)
    {
        try
        {
            var currency = await _currencyService.UpdateCurrencyAsync(id, request);
            return Ok(currency);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{code}/set-base")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> SetBaseCurrency(string code)
    {
        var success = await _currencyService.SetBaseCurrencyAsync(code);
        if (!success)
        {
            return BadRequest($"Cannot set '{code}' as base currency. Currency may not exist or be inactive.");
        }
        return Ok(new { message = $"Currency '{code}' set as base currency" });
    }

    [HttpPost("{id}/activate")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> ActivateCurrency(string id)
    {
        var success = await _currencyService.ActivateCurrencyAsync(id);
        if (!success)
        {
            return NotFound($"Currency with ID '{id}' not found");
        }
        return Ok(new { message = "Currency activated successfully" });
    }

    [HttpPost("{id}/deactivate")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> DeactivateCurrency(string id)
    {
        try
        {
            var success = await _currencyService.DeactivateCurrencyAsync(id);
            if (!success)
            {
                return NotFound($"Currency with ID '{id}' not found");
            }
            return Ok(new { message = "Currency deactivated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}