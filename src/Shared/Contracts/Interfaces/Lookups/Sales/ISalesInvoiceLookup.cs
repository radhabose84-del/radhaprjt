using Contracts.Dtos.Lookups.Sales;

namespace Contracts.Interfaces.Lookups.Sales
{
    public interface ISalesInvoiceLookup
    {
        Task<SalesInvoiceForEInvoiceDto?> GetInvoiceForEInvoiceAsync(string invoiceNumber, int unitId);
        Task<SalesInvoiceForEInvoiceDto?> GetInvoiceForEInvoiceByIdAsync(int invoiceId);
        Task RevertInvoiceStatusToPendingAsync(int invoiceId, CancellationToken ct);
    }
}
