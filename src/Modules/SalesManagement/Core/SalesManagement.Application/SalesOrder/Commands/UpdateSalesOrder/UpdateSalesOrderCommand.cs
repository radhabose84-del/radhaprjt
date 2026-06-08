using Contracts.Common;
using MediatR;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Commands.UpdateSalesOrder
{
    public class UpdateSalesOrderCommand : IRequest<ApiResponseDTO<int>>
    {
        public int Id { get; set; }
        public int IsActive { get; set; }

        // Quotation Reference (optional)
        public int? SalesQuotationHeaderId { get; set; }

        // Customer & Unit Details
        public int SalesGroupId { get; set; }
        public int? SalesSegmentId { get; set; }
        public int EnquiryType { get; set; }
        public int? UnitId { get; set; }
        public int PartyId { get; set; }
        public string? PartyAddress { get; set; }
        public int? AgentId { get; set; }
        public int? SubAgentId { get; set; }

        // Sales Order Type (cross-module FK → Finance.TransactionTypeMaster)
        public int? SalesOrderTypeId { get; set; }

        // Same-module FK → Sales.SalesOrderTypeMaster (nullable)
        public int? SalesOrderTypeMasterId { get; set; }

        // Same-module FK → Sales.SalesEnquiryHeader (nullable)
        public int? SalesEnquiryHeaderId { get; set; }

        // Reference numbers (nullable)
        public string? CustomerPoRefno { get; set; }
        public string? ComplaintRefno { get; set; }

        // Commercial Details
        public int? PaymentTypeId { get; set; }
        public int FreightTypeId { get; set; }
        public string? Remarks { get; set; }

        // MD Discount — when true, Rate + Document are mandatory
        public bool IsMdDiscountEnabled { get; set; }
        public decimal? MdDiscountRate { get; set; }
        public decimal? MdDiscountPercentage { get; set; }
        public decimal? MdDiscountValue { get; set; }
        public decimal? TotalDiscountValue { get; set; }
        public string? MdApprovalDocument { get; set; }

        // Agent Commission
        public int? AgentCommissionId { get; set; }
        public int AgentPaymentTermsId { get; set; }
        public int? AgentCommissionSlabId { get; set; }
        public decimal? CommissionRate { get; set; }
        public decimal? CommissionValue { get; set; }

        // File Attachments
        public string? VisitNotesAttachment { get; set; }
        public string? AgentPOAttachment { get; set; }

        // Derived Summary Fields
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

        // Line Items
        public List<UpdateSalesOrderDetailDto>? SalesOrderDetails { get; set; }

        // Applied discounts (max 3 — one per SlabType)
        public List<UpdateSalesOrderDiscountDto>? Discounts { get; set; }
    }
}
