using Contracts.Common;
using MediatR;
using SalesManagement.Application.Invoice.Dto;

namespace SalesManagement.Application.Invoice.Commands.UpdateInvoice
{
    public class UpdateInvoiceCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public int? AgentId { get; set; }
        public int? InvoiceTypeId { get; set; }
        public int? TransportMode { get; set; }
        public int? StatusId { get; set; }
        public string? VehicleNumber { get; set; }
        public string? TransporterName { get; set; }
        public string? LRNumber { get; set; }
        public DateOnly? LRDate { get; set; }

        // Header totals (calculated by frontend, validated by backend)
        public int TotalBags { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal TaxableValue { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalFreight { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal Insurance { get; set; }
        public decimal HandlingCharge { get; set; }
        public decimal TotalCharity { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TCSPercentage { get; set; }
        public decimal TCS { get; set; }
        public decimal RoundOff { get; set; }
        public decimal InvoiceAmountBeforeTCS { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }

        public List<UpdateInvoiceDetailDto>? Details { get; set; }
    }
}
