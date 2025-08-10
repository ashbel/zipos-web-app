namespace POS.Modules.Reporting.Services;

public interface IProfitabilityReportService
{
    Task<ProfitabilitySummaryDto> GetSummaryAsync(string organizationId, DateTime fromDate, DateTime toDate, string? branchId = null, CancellationToken ct = default);
    Task<IReadOnlyList<ProductProfitDto>> GetProductProfitAsync(string organizationId, DateTime fromDate, DateTime toDate, string? branchId = null, CancellationToken ct = default);
}

public record ProfitabilitySummaryDto(decimal Revenue, decimal Cogs, decimal GrossProfit, decimal GrossMarginPercent);
public record ProductProfitDto(string ProductId, string Name, decimal Revenue, decimal Cogs, decimal GrossProfit, decimal GrossMarginPercent);

