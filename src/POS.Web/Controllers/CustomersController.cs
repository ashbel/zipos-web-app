using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Customers.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;
    private readonly ICustomerHistoryService _historyService;
    private readonly ICustomerLoyaltyService _loyaltyService;

    public CustomersController(ICustomerService service, ICustomerHistoryService historyService, ICustomerLoyaltyService loyaltyService)
    {
        _service = service;
        _historyService = historyService;
        _loyaltyService = loyaltyService;
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
}

