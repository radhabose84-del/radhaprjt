using Contracts.Dtos.Stock;

namespace Contracts.Interfaces.Lookups.Sales
{
    public interface ISalesStockLedgerLookup
    {
        Task<bool> InsertAsync(List<SalesStockLedgerDto> entries, CancellationToken ct = default);
        Task<bool> DeleteByDocAsync(string docType, int docNo, CancellationToken ct = default);
    }
}
