using Contracts.Common;
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
        Task<NicEwbResultDto> GenerateEwbAsync(int eInvoiceHeaderId, string? transporterId, string? transporterName,
            string? transMode, int distance, string? transDocNo, string? transDocDt,
            string? vehicleNo, string? vehicleType, CancellationToken ct = default);

        /// <summary>
        /// Cancels an IRN within 24 hours of generation.
        /// CnlRsn: "1"=Duplicate, "2"=Data entry mistake, "3"=Order cancelled, "4"=Others
        /// </summary>
        Task<NicCancelIrnResultDto> CancelIrnAsync(int eInvoiceHeaderId, string cnlRsn,
            string? cnlRem = null, CancellationToken ct = default);

        /// <summary>
        /// Cancels an e-Waybill within 24 hours of generation.
        /// Must cancel EWB before cancelling IRN if both exist.
        /// </summary>
        Task<NicCancelEwbResultDto> CancelEwbAsync(int eInvoiceHeaderId, int cancelRsnCode,
            string? cancelRmrk = null, CancellationToken ct = default);

        /// <summary>
        /// Fetches IRN details from NIC API by IRN number.
        /// GET /eicore/v1.03/Invoice/irn/{irn}
        /// </summary>
        Task<ApiResponseDTO<object>> GetIrnDetailsAsync(int eInvoiceHeaderId, CancellationToken ct = default);

        /// <summary>
        /// Fetches e-Waybill details from NIC API by IRN number.
        /// GET /eiewb/v1.03/ewaybill/irn/{irn}
        /// </summary>
        Task<ApiResponseDTO<object>> GetEwbDetailsByIrnAsync(int eInvoiceHeaderId, CancellationToken ct = default);
    }
}
