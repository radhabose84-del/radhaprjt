using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesQuotation.Dto;

namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment
{
    public class CreateSalesQuotationAmendmentCommand : IRequest<ApiResponseDTO<int>>
    {
        public int SalesQuotationHeaderId { get; set; }
        public string? Reason { get; set; }

        // Header-level Summary Fields
        public decimal FreightCharges { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TotalBasicAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetTaxableAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }

        public List<CreateSalesQuotationAmendmentDetailDto>? AmendmentDetails { get; set; }
    }
}
