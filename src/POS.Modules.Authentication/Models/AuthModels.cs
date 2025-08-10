namespace POS.Modules.Authentication.Models;

public record LoginRequest(string OrganizationId, string Email, string Password);

public record AuthResult(
    bool Success,
    string? AccessToken,
    string? RefreshToken,
    DateTime? ExpiresAt,
    string? Error
);

