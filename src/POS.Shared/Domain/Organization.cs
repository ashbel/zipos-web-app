using POS.Shared.Domain.ValueObjects;

namespace POS.Shared.Domain;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SubscriptionPlan { get; set; } = "Basic";
    public Address? Address { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? SubscriptionExpiresAt { get; set; }
    
    // Note: Navigation properties to Branches and Users are not included
    // since they exist in separate tenant schemas. Use repository patterns
    // to query tenant-specific data with proper schema context.
}