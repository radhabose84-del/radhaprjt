using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesQuotationAmendmentHeader : BaseEntity
    {
        public int SalesQuotationHeaderId { get; set; }
        public int UnitId { get; set; }                  // Cross-module FK (UserManagement)
        public string? AmendmentNo { get; set; }
        public int RevisionNumber { get; set; }
        public DateOnly AmendmentDate { get; set; }
        public string? Reason { get; set; }

        // Approval Status (same-module FK to MiscMaster)
        public int? StatusId { get; set; }

        // Header-level Summary Fields (captured at amendment time)
        public decimal FreightCharges { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TotalBasicAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetTaxableAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }

        // Who approved/rejected
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }

        // Navigation Properties
        public SalesQuotationHeader? SalesQuotationHeader { get; set; }
        public MiscMaster? StatusMisc { get; set; }

        // Child collection
        public ICollection<SalesQuotationAmendmentDetail>? SalesQuotationAmendmentDetails { get; set; }
    }
}
