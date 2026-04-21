using Contracts.Dtos.Lookups.Finance;

namespace Contracts.Interfaces.Lookups.Finance
{
    public interface IEWaybillLookup
    {
        Task<EWaybillLookupDto?> GetByInvoiceAsync(string invoiceNo, int unitId, CancellationToken ct = default);
        Task<EWaybillLookupDto?> GetByDCAsync(string deliveryNumber, int unitId, CancellationToken ct = default);
    }
}
