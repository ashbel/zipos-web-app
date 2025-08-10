using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Modules.Sales.Services;
using System.Text;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _sales;
    private readonly IReceiptService _receipt;

    public SalesController(ISalesService sales, IReceiptService receipt)
    {
        _sales = sales;
        _receipt = receipt;
    }

    [HttpPost("cart")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> CreateCart([FromQuery] string organizationId, [FromQuery] string userId, [FromQuery] string branchId, CancellationToken ct)
    {
        var cart = await _sales.CreateCartAsync(organizationId, userId, branchId, ct);
        return Ok(cart);
    }

    [HttpPost("cart/{cartId}/items")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> AddItem([FromQuery] string organizationId, [FromRoute] string cartId, [FromQuery] string productId, [FromQuery] string name, [FromQuery] decimal quantity, [FromQuery] decimal unitPrice, [FromQuery] decimal discount, CancellationToken ct)
    {
        var cart = await _sales.AddItemAsync(organizationId, cartId, productId, name, quantity, unitPrice, discount, ct);
        return Ok(cart);
    }

    [HttpDelete("cart/{cartId}/items/{itemId}")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> RemoveItem([FromQuery] string organizationId, [FromRoute] string cartId, [FromRoute] string itemId, CancellationToken ct)
    {
        var cart = await _sales.RemoveItemAsync(organizationId, cartId, itemId, ct);
        return Ok(cart);
    }

    [HttpPost("cart/{cartId}/checkout")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> Checkout([FromQuery] string organizationId, [FromRoute] string cartId, [FromBody] IEnumerable<PaymentRequest> payments, CancellationToken ct)
    {
        var sale = await _sales.CheckoutAsync(organizationId, cartId, payments, ct);
        var receipt = await _receipt.GenerateReceiptAsync(organizationId, sale.Id, ct);
        return Ok(new { sale, receipt });
    }
}

