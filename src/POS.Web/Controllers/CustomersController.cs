using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Customers.Services;
using POS.Shared.Domain;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;
    private readonly ICustomerHistoryService _historyService;
    private readonly ICustomerLoyaltyService _loyaltyService;
    private readonly ICustomerCreditService _creditService;
    private readonly ICustomerAnalyticsService _analyticsService;

    public CustomersController(ICustomerService service, ICustomerHistoryService historyService, ICustomerLoyaltyService loyaltyService, ICustomerAnalyticsService analyticsService, ICustomerCreditService creditService)
    {
        _service = service;
        _historyService = historyService;
        _loyaltyService = loyaltyService;
        _analyticsService = analyticsService;
        _creditService = creditService;
    }

    [HttpGet]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> Search([FromQuery] string organizationId, [FromQuery] string? q, CancellationToken ct)
    {
        var items = await _service.SearchAsync(organizationId, q, ct);
        return Ok(items);
    }

    [HttpPost]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> Create([FromQuery] string organizationId, [FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var item = await _service.CreateAsync(organizationId, request, ct);
        return Ok(item);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> Update([FromQuery] string organizationId, [FromRoute] string id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var item = await _service.UpdateAsync(organizationId, id, request, ct);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> Delete([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var ok = await _service.DeleteAsync(organizationId, id, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("{id}/purchases")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> GetPurchases([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var sales = await _historyService.GetPurchaseHistoryAsync(organizationId, id, ct);
        return Ok(sales);
    }

    [HttpGet("{id}/loyalty")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> GetLoyalty([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var cl = await _loyaltyService.GetAsync(organizationId, id, ct);
        return Ok(cl);
    }

    [HttpGet("loyalty/tiers")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> GetLoyaltyTiers([FromQuery] string organizationId, CancellationToken ct)
    {
        var tiers = await _loyaltyService.GetTiersAsync(organizationId, ct);
        return Ok(tiers);
    }

    [HttpPost("loyalty/tiers")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> UpsertLoyaltyTier([FromQuery] string organizationId, [FromBody] LoyaltyTierDefinition tier, CancellationToken ct)
    {
        var saved = await _loyaltyService.CreateOrUpdateTierAsync(organizationId, tier, ct);
        return Ok(saved);
    }

    [HttpGet("{id}/analytics")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> GetAnalytics([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var a = await _analyticsService.GetAnalyticsAsync(organizationId, id, ct);
        return Ok(a);
    }

    [HttpGet("{id}/credit")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> GetCredit([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var c = await _creditService.GetAsync(organizationId, id, ct);
        return Ok(c);
    }

    [HttpPost("{id}/credit/limit")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> SetCreditLimit([FromQuery] string organizationId, [FromRoute] string id, [FromBody] SetCreditLimitRequest request, CancellationToken ct)
    {
        var c = await _creditService.SetLimitAsync(organizationId, id, request.CreditLimit, ct);
        return Ok(c);
    }

    [HttpPost("{id}/credit/payment")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> RecordCreditPayment([FromQuery] string organizationId, [FromRoute] string id, [FromBody] RecordCreditPaymentRequest request, CancellationToken ct)
    {
        var ok = await _creditService.RecordPaymentAsync(organizationId, id, request.Amount, request.Reference, ct);
        return ok ? Ok() : BadRequest();
    }

    [HttpGet("{id}/credit/transactions")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)] // placeholder
    public async Task<IActionResult> GetCreditTransactions([FromQuery] string organizationId, [FromRoute] string id, CancellationToken ct)
    {
        var txns = await _creditService.GetTransactionsAsync(organizationId, id, ct);
        return Ok(txns);
    }

    public record SetCreditLimitRequest(decimal CreditLimit);
    public record RecordCreditPaymentRequest(decimal Amount, string? Reference);
}

