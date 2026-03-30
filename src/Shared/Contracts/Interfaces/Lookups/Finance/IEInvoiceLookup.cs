using Contracts.Dtos.Lookups.Finance;

namespace Contracts.Interfaces.Lookups.Finance
{
    public interface IEInvoiceLookup
    {
        Task<EInvoiceLookupDto?> GetByInvoiceAsync(string invoiceNo, int unitId, CancellationToken ct = default);
    }
}
