using Contracts.Common;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.GenerateIrn
{
    public class GenerateIrnCommand : IRequest<ApiResponseDTO<NicIrnResultDto>>
    {
        public int EInvoiceHeaderId { get; set; }

        // ── Optional e-Waybill transport details (Case 1: IRN + EWB together) ──
        // When provided, NIC generates both IRN and e-Waybill in a single call.
        // When null/empty, only IRN is generated (Case 2: separate calls).
        public string? TransId { get; set; }
        public string? TransName { get; set; }
        public string? TransMode { get; set; }
        public int? Distance { get; set; }
        public string? TransDocNo { get; set; }
        public string? TransDocDt { get; set; }
        public string? VehNo { get; set; }
        public string? VehType { get; set; }
    }
}
