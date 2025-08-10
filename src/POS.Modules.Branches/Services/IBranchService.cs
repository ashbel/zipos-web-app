using POS.Shared.Domain;

namespace POS.Modules.Branches.Services;

public interface IBranchService
{
    Task<Branch> CreateBranchAsync(string organizationId, CreateBranchRequest request, CancellationToken ct = default);
    Task<Branch?> UpdateBranchAsync(string organizationId, string branchId, UpdateBranchRequest request, CancellationToken ct = default);
    Task<bool> DeleteBranchAsync(string organizationId, string branchId, CancellationToken ct = default);
    Task<IReadOnlyList<Branch>> GetBranchesAsync(string organizationId, CancellationToken ct = default);
}

public record CreateBranchRequest(string Name, string Code, string? ContactPhone, string? ContactEmail, string TimeZone, string Currency, decimal TaxRate, bool TaxInclusive);
public record UpdateBranchRequest(string? Name, string? ContactPhone, string? ContactEmail, string? TimeZone, string? Currency, decimal? TaxRate, bool? TaxInclusive, bool? IsActive);

