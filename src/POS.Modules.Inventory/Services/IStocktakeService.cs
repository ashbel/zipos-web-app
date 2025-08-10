using POS.Shared.Domain;

namespace POS.Modules.Inventory.Services;

public interface IStocktakeService
{
    Task<StocktakeSession> StartSessionAsync(string organizationId, string branchId, string startedBy, CancellationToken ct = default);
    Task<StocktakeLine> UpsertCountAsync(string organizationId, string sessionId, string productId, decimal countedQty, CancellationToken ct = default);
    Task<StocktakeSession> FinalizeSessionAsync(string organizationId, string sessionId, string finalizedBy, bool createAdjustments, CancellationToken ct = default);
    Task<IReadOnlyList<StocktakeLine>> GetSessionLinesAsync(string organizationId, string sessionId, CancellationToken ct = default);
}

