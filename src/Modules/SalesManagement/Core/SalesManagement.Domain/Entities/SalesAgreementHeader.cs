using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesAgreementHeader : BaseEntity
    {
        public string? AgreementNo { get; set; }

        // Approval status — FK → Sales.MiscMaster (same-module) where MiscTypeId=36 (ApprovalStatus).
        // Defaults to Pending on create; allowed transitions enforced in Update validator.
        public int StatusId { get; set; }

        public DateOnly ValidFrom { get; set; }
        public DateOnly ValidTo { get; set; }

        // Cross-module FK → PartyManagement (resolved via ICustomerLookup). No DB FK constraint.
        public int CustomerId { get; set; }

        // Same-module FK → Sales.SalesGroup.
        public int SalesGroupId { get; set; }

        // Cross-module FK → PurchaseManagement (resolved via IPaymentTermLookup). No DB FK constraint.
        public int PaymentTermsId { get; set; }

        public string? Remarks { get; set; }

        // Customer PO Reference number (free text)
        public string? CustomerPoRefno { get; set; }

        // Agent PO Attachment — uploaded file name (path resolved via company/unit at runtime)
        public string? AgentPOAttachment { get; set; }

        // Capturing Unit — set by handler from JWT/IP context at creation time (cross-module FK to UserManagement, no DB FK constraint).
        public int? UnitId { get; set; }

        // Same-module navigation properties
        public MiscMaster? StatusMisc { get; set; }
        public SalesGroup? SalesGroup { get; set; }

        // Child collection
        public ICollection<SalesAgreementDetail>? SalesAgreementDetails { get; set; }

        // Reverse navigation — SalesOrderHeaders raised against this agreement
        public ICollection<SalesOrderHeader>? SalesOrderHeaders { get; set; }
    }
}
