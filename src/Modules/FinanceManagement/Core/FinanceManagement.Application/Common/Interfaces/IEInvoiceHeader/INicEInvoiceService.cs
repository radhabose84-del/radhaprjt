using FinanceManagement.Application.EInvoiceHeader.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader
{
    public interface INicEInvoiceService
    {
        /// <summary>
        /// Authenticates with the NIC API, builds the GST e-invoice JSON from the stored
        /// EInvoiceHeader + EInvoiceDetail records, encrypts it with the session key,
        /// calls the NIC Generate IRN endpoint, decrypts and returns the result.
        /// </summary>
        Task<NicIrnResultDto> GenerateIrnAsync(int eInvoiceHeaderId, CancellationToken ct = default);
    }
}
