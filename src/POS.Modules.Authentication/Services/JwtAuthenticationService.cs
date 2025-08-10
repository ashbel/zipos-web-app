using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using POS.Infrastructure.Data;
using POS.Modules.Authentication.Models;
using POS.Shared.Infrastructure;

namespace POS.Modules.Authentication.Services;

public class JwtAuthenticationService : IAuthenticationService
{
    private readonly POSDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IConfiguration _config;

    public JwtAuthenticationService(POSDbContext db, ITenantContext tenantContext, IConfiguration config)
    {
        _db = db;
        _tenantContext = tenantContext;
        _config = config;
    }

    public async Task<AuthResult> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        _tenantContext.SetTenant(request.OrganizationId);

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive, cancellationToken);
        if (user == null)
        {
            return new AuthResult(false, null, null, null, "Invalid credentials");
        }

        // NOTE: Replace with proper password hashing (e.g., BCrypt). For now, compare SHA256 in demo.
        // Assuming PasswordHash is hex of SHA256.
        var inputHash = ComputeSha256(request.Password);
        if (!string.Equals(user.PasswordHash, inputHash, StringComparison.OrdinalIgnoreCase))
        {
            return new AuthResult(false, null, null, null, "Invalid credentials");
        }

        var (accessToken, expiresAt) = GenerateJwt(user.Id, request.OrganizationId);
        var refreshToken = Guid.NewGuid().ToString("N");

        // Persist refresh token
        var trackedUser = await _db.Users.FirstAsync(u => u.Id == user.Id, cancellationToken);
        trackedUser.RefreshToken = refreshToken;
        trackedUser.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResult(true, accessToken, refreshToken, expiresAt, null);
    }

    public async Task<AuthResult> RefreshTokenAsync(string organizationId, string refreshToken, CancellationToken cancellationToken = default)
    {
        _tenantContext.SetTenant(organizationId);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiresAt > DateTime.UtcNow, cancellationToken);
        if (user == null)
        {
            return new AuthResult(false, null, null, null, "Invalid refresh token");
        }

        var (accessToken, expiresAt) = GenerateJwt(user.Id, organizationId);
        var newRefresh = Guid.NewGuid().ToString("N");
        user.RefreshToken = newRefresh;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResult(true, accessToken, newRefresh, expiresAt, null);
    }

    public Task LogoutAsync(string organizationId, string userId, CancellationToken cancellationToken = default)
    {
        _tenantContext.SetTenant(organizationId);
        return LogoutInternalAsync(userId, cancellationToken);
    }

    private async Task LogoutInternalAsync(string userId, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiresAt = null;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private (string token, DateTime expiresAt) GenerateJwt(string userId, string organizationId)
    {
        var key = _config["Jwt:Key"] ?? "dev-secret";
        var issuer = _config["Jwt:Issuer"] ?? "zipos";
        var audience = _config["Jwt:Audience"] ?? "zipos-clients";
        var expires = DateTime.UtcNow.AddMinutes(30);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new("org", organizationId)
        };

        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims, expires: expires, signingCredentials: signingCredentials);
        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

