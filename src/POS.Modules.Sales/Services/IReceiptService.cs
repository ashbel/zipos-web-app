using POS.Shared.Domain;

namespace POS.Modules.Sales.Services;

public interface IReceiptService
{
    Task<string> GenerateReceiptAsync(string organizationId, string saleId, CancellationToken ct = default);
}

