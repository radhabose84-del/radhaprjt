using Contracts.Dtos.Lookups.Logistics;
using Contracts.Dtos.Lookups.Party;
using SalesManagement.Application.SalesOrder.Dto;

namespace SalesManagement.Application.SalesOrder.Queries.GetPendingSalesOrderById
{
    public class PendingSalesOrderByIdDto
    {
        // Header fields
        public int Id { get; set; }
        public string? SalesOrderNo { get; set; }
        public DateOnly OrderDate { get; set; }

        // Quotation Reference
        public int? SalesQuotationHeaderId { get; set; }

        // Customer & Unit Details
        public int SalesGroupId { get; set; }
        public string? SalesGroupName { get; set; }
        public int? SalesSegmentId { get; set; }
        public string? SegmentName { get; set; }
        public int EnquiryType { get; set; }
        public string? EnquiryTypeName { get; set; }
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public string? PartyAddress { get; set; }
        public int? SalesFreightId { get; set; }
        public FreightMasterLookupDto? SalesFreight { get; set; }
        public List<PartyAddressLookupDto>? PartyAddresses { get; set; }
        public int? AgentId { get; set; }
        public string? AgentName { get; set; }
        public int? SubAgentId { get; set; }
        public string? SubAgentName { get; set; }

        // Sales Order Type
        public int? SalesOrderTypeId { get; set; }
        public string? SalesOrderTypeName { get; set; }

        // Order Unit (captured from JWT at creation)
        public int? OrderUnitId { get; set; }
        public string? OrderUnitName { get; set; }

        // Commercial Details
        public int? PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }
        public int FreightTypeId { get; set; }
        public string? FreightTypeName { get; set; }
        public string? Remarks { get; set; }

        // MD Discount
        public bool IsMdDiscountEnabled { get; set; }
        public decimal? MdDiscountRate { get; set; }
        public decimal? MdDiscountPercentage { get; set; }
        public decimal? MdDiscountValue { get; set; }
        public decimal? TotalDiscountValue { get; set; }
        public string? MdApprovalDocument { get; set; }
        public string? MdApprovalDocumentPath { get; set; }

        // Agent Commission
        public int? AgentCommissionId { get; set; }
        public int AgentPaymentTermsId { get; set; }
        public int? AgentCommissionSlabId { get; set; }
        public decimal? CommissionRate { get; set; }
        public decimal? CommissionValue { get; set; }

        // File Attachments
        public string? VisitNotesAttachment { get; set; }
        public string? VisitNotesAttachmentPath { get; set; }
        public string? AgentPOAttachment { get; set; }
        public string? AgentPOAttachmentPath { get; set; }

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

        // Status
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }

        // Workflow fields
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }

        // Detail lines
        public List<SalesOrderDetailDto>? SalesOrderDetails { get; set; }

        // Applied discounts (populated in GetById)
        public List<SalesOrderDiscountDto>? Discounts { get; set; }
    }
}
