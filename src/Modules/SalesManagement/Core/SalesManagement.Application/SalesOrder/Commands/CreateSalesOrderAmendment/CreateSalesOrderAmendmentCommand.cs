using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment
{
    public class CreateSalesOrderAmendmentCommand : IRequest<ApiResponseDTO<int>>
    {
        public int SalesOrderHeaderId { get; set; }
        public string? Reason { get; set; }

        // Header-level Summary Fields
        public int TotalBags { get; set; }
        public decimal TotalWeightKgs { get; set; }
        public decimal TotalDiscountPerKg { get; set; }
        public decimal ItemValue { get; set; }
        public decimal TotalFreight { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal GSTPercentage { get; set; }
        public decimal TotalGST { get; set; }
        public decimal TotalWithGST { get; set; }
        public decimal TCSPercentage { get; set; }
        public decimal TotalTCS { get; set; }
        public decimal FinalAmount { get; set; }

        // Agent Commission snapshot
        public int? AgentCommissionId { get; set; }
        public int? AgentCommissionSlabId { get; set; }
        public int AgentPaymentTermsId { get; set; }
        public decimal? CommissionRate { get; set; }
        public decimal? CommissionValue { get; set; }

        // Discount snapshot
        public decimal? MdDiscountValue { get; set; }
        public decimal? TotalDiscountValue { get; set; }

        public List<CreateSalesOrderAmendmentDetailDto>? AmendmentDetails { get; set; }

        // Applied discounts snapshot (max 3 — one per SlabType)
        public List<CreateSalesOrderAmendmentDiscountDto>? Discounts { get; set; }
    }
}
