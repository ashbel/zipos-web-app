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

    [HttpPost("cart/{cartId}/apply-promotions")]
    [Authorize(Policy = "CanManageUsers")] // placeholder policy
    public async Task<IActionResult> ApplyPromotions([FromQuery] string organizationId, [FromRoute] string cartId, [FromBody] ApplyPromotionsRequest request, CancellationToken ct)
    {
        var cart = await _sales.ApplyPromotionsAsync(organizationId, cartId, request.PromoCode, request.CustomerId, ct);
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
    public async Task<IActionResult> Checkout([FromQuery] string organizationId, [FromRoute] string cartId, [FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var sale = await _sales.CheckoutAsync(organizationId, cartId, request.Payments, request.CustomerId, ct);
        var receipt = await _receipt.GenerateReceiptAsync(organizationId, sale.Id, ct);
        return Ok(new { sale, receipt });
    }

    public record CheckoutRequest(IEnumerable<PaymentRequest> Payments, string? CustomerId);
    public record ApplyPromotionsRequest(string? PromoCode, string? CustomerId);

    [HttpPost("sales/{saleId}/refunds")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanManageUsers)]
    public async Task<IActionResult> Refund([FromQuery] string organizationId, [FromRoute] string saleId, [FromQuery] string reason, [FromQuery] string processedBy, [FromBody] IEnumerable<RefundLineRequest> items, CancellationToken ct)
    {
        var refund = await _sales.ProcessRefundAsync(organizationId, saleId, items, reason, processedBy, ct);
        return Ok(refund);
    }

    [HttpPost("refunds/{refundId}/approve")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanApproveRefunds)]
    public async Task<IActionResult> ApproveRefund([FromQuery] string organizationId, [FromRoute] string refundId, [FromQuery] string approvedBy, CancellationToken ct)
    {
        var ok = await _sales.ApproveRefundAsync(organizationId, refundId, approvedBy, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("refunds/{refundId}/reject")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanApproveRefunds)]
    public async Task<IActionResult> RejectRefund([FromQuery] string organizationId, [FromRoute] string refundId, [FromQuery] string rejectedBy, CancellationToken ct)
    {
        var ok = await _sales.RejectRefundAsync(organizationId, refundId, rejectedBy, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("sales/{saleId}/receipt")]
    [Authorize(Policy = POS.Modules.Authentication.Authorization.Policies.CanReprintReceipts)]
    public async Task<IActionResult> GetReceipt([FromQuery] string organizationId, [FromRoute] string saleId, CancellationToken ct)
    {
        var receipt = await _receipt.GenerateReceiptAsync(organizationId, saleId, ct);
        return Ok(new { saleId, receipt });
    }
}

