using FinanceManagement.Application.EInvoiceHeader.Dto;

namespace FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader
{
    public interface INicEInvoiceService
    {
        /// <summary>
        /// Authenticates with the NIC API, builds the GST e-invoice JSON from the stored
        /// EInvoiceHeader + EInvoiceDetail records, encrypts it with the session key,
        /// calls the NIC Generate IRN endpoint, decrypts and returns the result.
        /// When ewbDetails is provided, includes EwbDtls in the payload so NIC generates
        /// both IRN and e-Waybill in a single call.
        /// </summary>
        Task<NicIrnResultDto> GenerateIrnAsync(int eInvoiceHeaderId,
            EwbTransportDetails? ewbDetails = null, CancellationToken ct = default);

        /// <summary>
        /// Generates an e-Waybill from an existing IRN by calling the NIC e-Waybill API
        /// with transport details. The EInvoiceHeader must already have a valid IrnNumber.
        /// </summary>
        Task<NicEwbResultDto> GenerateEwbAsync(int eInvoiceHeaderId, string transporterId, string transporterName,
            string transMode, int distance, string transDocNo, string transDocDt,
            string vehicleNo, string vehicleType, CancellationToken ct = default);
    }
}
