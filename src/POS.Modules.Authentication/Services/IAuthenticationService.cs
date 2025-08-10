using POS.Modules.Authentication.Models;

namespace POS.Modules.Authentication.Services;

public interface IAuthenticationService
{
    Task<AuthResult> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResult> RefreshTokenAsync(string organizationId, string refreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string organizationId, string userId, CancellationToken cancellationToken = default);
}

