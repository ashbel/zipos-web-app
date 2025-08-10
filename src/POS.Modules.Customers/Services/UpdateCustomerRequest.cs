namespace POS.Modules.Customers.Services;

public record UpdateCustomerRequest(string? Name, string? Email, string? Phone, string? TaxId, string? Notes, string? LoyaltyTier, bool? IsActive);