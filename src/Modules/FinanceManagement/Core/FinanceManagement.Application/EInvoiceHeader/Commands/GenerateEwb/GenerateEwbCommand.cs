using Contracts.Common;
using FinanceManagement.Application.EInvoiceHeader.Dto;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.GenerateEwb
{
    public class GenerateEwbCommand : IRequest<ApiResponseDTO<NicEwbResultDto>>
    {
        public int EInvoiceHeaderId { get; set; }
        public string? TransporterId { get; set; }
        public string? TransporterName { get; set; }
        public string? TransMode { get; set; }
        public int Distance { get; set; }
        public string? TransDocNo { get; set; }
        public string? TransDocDt { get; set; }
        public string? VehicleNo { get; set; }
        public string? VehicleType { get; set; }
    }
}
