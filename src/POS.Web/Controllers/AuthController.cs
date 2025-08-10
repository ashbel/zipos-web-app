using Microsoft.AspNetCore.Mvc;
using POS.Modules.Authentication.Models;
using POS.Modules.Authentication.Services;

namespace POS.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _auth;
    private readonly IConfiguration _config;

    public AuthController(IAuthenticationService auth, IConfiguration config)
    {
        _auth = auth;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _auth.AuthenticateAsync(request, ct);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromQuery] string organizationId, [FromQuery] string refreshToken, CancellationToken ct)
    {
        var result = await _auth.RefreshTokenAsync(organizationId, refreshToken, ct);
        if (!result.Success) return Unauthorized(result);
        return Ok(result);
    }
}

