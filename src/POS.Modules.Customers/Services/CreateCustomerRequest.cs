namespace POS.Modules.Customers.Services;

public record CreateCustomerRequest(string Name, string? Email, string? Phone, string? TaxId, string? Notes);