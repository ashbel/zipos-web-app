using POS.Shared.Domain.ValueObjects;

namespace POS.Shared.Domain;

public class Branch : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Address? Address { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string TimeZone { get; set; } = "UTC";
    public string Currency { get; set; } = "USD";
    public decimal TaxRate { get; set; } = 0.0m;
    public bool TaxInclusive { get; set; } = false;
    
    // Settings as JSON
    public string Settings { get; set; } = "{}";
    
    // Note: No direct Organization navigation property needed since schema isolation
    // provides the tenant boundary. Organization info can be retrieved from tenant context.
}