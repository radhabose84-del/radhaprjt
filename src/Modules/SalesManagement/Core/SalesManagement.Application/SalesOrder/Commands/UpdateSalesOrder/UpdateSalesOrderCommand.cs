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
        public int UnitId { get; set; }
        public int PartyId { get; set; }
        public string? PartyAddress { get; set; }
        public int? AgentId { get; set; }
        public int? SubAgentId { get; set; }

        // Sales Order Type (cross-module FK → Finance.TransactionTypeMaster)
        public int? SalesOrderTypeId { get; set; }

        // Commercial Details
        public int? DiscountPlanId { get; set; }
        public int PaymentTermsId { get; set; }
        public int? PaymentTypeId { get; set; }
        public int FreightTypeId { get; set; }
        public int? CountListId { get; set; }
        public string? Remarks { get; set; }

        // MD Discount — when true, Rate + Document are mandatory
        public bool IsMdDiscountEnabled { get; set; }
        public decimal? MdDiscountRate { get; set; }
        public string? MdApprovalDocument { get; set; }

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
