namespace POS.Shared.Infrastructure;

public interface ISchemaService
{
    Task CreateTenantSchemaAsync(string organizationId, CancellationToken cancellationToken = default);
    Task DeleteTenantSchemaAsync(string organizationId, CancellationToken cancellationToken = default);
    Task<bool> SchemaExistsAsync(string organizationId, CancellationToken cancellationToken = default);
    Task MigrateTenantSchemaAsync(string organizationId, CancellationToken cancellationToken = default);
    string GetSchemaName(string organizationId);
}