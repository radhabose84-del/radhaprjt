using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesOrderHeader : BaseEntity
    {
        public string? SalesOrderNo { get; set; }
        public DateOnly OrderDate { get; set; }

        // Customer & Unit Details
        public int SalesGroupId { get; set; }
        public int? SalesSegmentId { get; set; }
        public int EnquiryType { get; set; }            // 1=Unit, 2=Combined
        public int UnitId { get; set; }                  // Cross-module FK (UserManagement)
        public int PartyId { get; set; }                 // Cross-module FK (PartyManagement)
        public string? PartyAddress { get; set; }           // Free-text address (numbers, letters, special chars allowed)
        public int? AgentId { get; set; }                // Cross-module FK (PartyManagement) — nullable
        public int? SubAgentId { get; set; }             // Cross-module FK (PartyManagement) — nullable

        // Sales Order Type (cross-module FK → Finance.TransactionTypeMaster)
        public int? SalesOrderTypeId { get; set; }

        // Order Unit — captured from JWT at creation time (cross-module FK)
        public int? OrderUnitId { get; set; }

        // Commercial Details
        public int? PaymentTypeId { get; set; }
        public int FreightTypeId { get; set; }
        public int? CountListId { get; set; }
        public string? Remarks { get; set; }

        // MD Discount — when checkbox enabled, Rate + Document are mandatory
        public bool IsMdDiscountEnabled { get; set; }
        public decimal? MdDiscountRate { get; set; }
        public decimal? MdDiscountPercentage { get; set; }
        public decimal? MdDiscountValue { get; set; }
        public decimal? TotalDiscountValue { get; set; }
        public string? MdApprovalDocument { get; set; }

        // Agent Commission — all fields optional; slab/rate/value snapshotted only when resolved
        public int? AgentCommissionId { get; set; }          // FK → Sales.AgentCommissionConfig (optional)
        public int AgentPaymentTermsId { get; set; }         // Lookup — no DB FK (resolved via IPaymentTermLookup)
        public int? AgentCommissionSlabId { get; set; }      // FK → Sales.AgentCommissionSlab (optional)
        public decimal? CommissionRate { get; set; }         // Applied rate snapshot (optional)
        public decimal? CommissionValue { get; set; }        // Calculated commission snapshot (optional)

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

        // Quotation Reference (nullable - same-module FK)
        public int? SalesQuotationHeaderId { get; set; }

        // Approval Status (same-module FK to MiscMaster)
        public int? StatusId { get; set; }

        // Revision tracking (incremented on each approved amendment)
        public int RevisionNumber { get; set; }  // Default 0 (original)

        // Cancelled fields
        public DateTimeOffset? CancelledDate { get; set; }
        public string? CancelledByName { get; set; }
        public string? CancelledIP { get; set; }

        // ForeClosed fields
        public DateTimeOffset? ForeClosedDate { get; set; }
        public string? ForeClosedByName { get; set; }
        public string? ForeClosedIP { get; set; }

        // Navigation Properties (Same-Module FKs only)
        public SalesQuotationHeader? SalesQuotation { get; set; }
        public SalesGroup? SalesGroup { get; set; }
        public SalesSegment? SalesSegment { get; set; }
        public MiscMaster? EnquiryTypeMisc { get; set; }
        public MiscMaster? PaymentType { get; set; }
        public MiscMaster? FreightType { get; set; }
        public MiscMaster? CountList { get; set; }
        public MiscMaster? StatusMisc { get; set; }
        public AgentCommissionConfig? AgentCommissionConfig { get; set; }
        public AgentCommissionSlab? AgentCommissionSlab { get; set; }

        // Child collection
        public ICollection<SalesOrderDetail>? SalesOrderDetails { get; set; }
        public ICollection<SalesOrderDiscount>? SalesOrderDiscounts { get; set; }

        // Reverse navigation (DispatchAdvice)
        public ICollection<DispatchAdviceHeader>? DispatchAdviceHeaders { get; set; }

        // Reverse navigation (Amendment)
        public ICollection<SalesOrderAmendmentHeader>? SalesOrderAmendmentHeaders { get; set; }

        // Reverse navigation (ProformaInvoice)
        public ICollection<ProformaInvoice>? ProformaInvoices { get; set; }
    }
}
