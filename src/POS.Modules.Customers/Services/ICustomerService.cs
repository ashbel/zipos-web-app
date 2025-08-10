using POS.Shared.Domain;

namespace POS.Modules.Customers.Services;

public interface ICustomerService
{
    Task<Customer> CreateAsync(string organizationId, CreateCustomerRequest request, CancellationToken ct = default);
    Task<Customer?> UpdateAsync(string organizationId, string customerId, UpdateCustomerRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(string organizationId, string customerId, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> SearchAsync(string organizationId, string? query, CancellationToken ct = default);
}