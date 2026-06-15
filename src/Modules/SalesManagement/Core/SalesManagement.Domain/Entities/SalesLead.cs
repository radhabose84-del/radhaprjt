using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesLead : BaseEntity
    {
        public string? LeadNo { get; set; }

        // Party identification
        public int? PartyId { get; set; }               // cross-module FK → PartyManagement (no DB constraint)
        public string? ProspectCompanyName { get; set; }
        public int? CityId { get; set; }                // cross-module FK → UserManagement (no DB constraint)

        // Contact person
        public string? ContactName { get; set; }
        public string? MobileNumber { get; set; }
        public string? EmailId { get; set; }
        public int? ContactId { get; set; }             // same-module FK → Sales.SalesContact

        // Requirement snapshot
        public int? ItemId { get; set; }                // cross-module FK → InventoryManagement (no DB constraint)
        public int? VariantId { get; set; }              // cross-module FK → InventoryManagement (no DB constraint)
        public int? UomId { get; set; }                  // cross-module FK → InventoryManagement (no DB constraint)
        public decimal? RequirementQty { get; set; }
        public DateOnly? ExpectedDate { get; set; }
        public string? Remarks { get; set; }

        // Ownership & source
        public int? LeadSourceId { get; set; }          // same-module FK → Sales.MiscMaster
        public int MarketingOfficerId { get; set; }     // same-module FK → Sales.MarketingOfficer
        public DateTimeOffset InteractionDate { get; set; }

        // Closure (Close Lead) — a lead is "closed" when ClosureTypeId is set
        public int? ClosureTypeId { get; set; }          // same-module FK → Sales.MiscMaster (Won/Lost/Not Interested/Duplicate/Invalid Lead)
        public int? ClosureReasonId { get; set; }        // same-module FK → Sales.MiscMaster (null for Won)
        public int? ConvertWonLeadToId { get; set; }     // same-module FK → Sales.MiscMaster (Enquiry/Quotation/Sales Order; only for Won)
        public string? ClosureRemarks { get; set; }
        public DateTimeOffset? ClosureDate { get; set; }

        // Same-module navigation properties
        public SalesContact? Contact { get; set; }
        public MiscMaster? LeadSource { get; set; }
        public MarketingOfficer? MarketingOfficer { get; set; }
        public MiscMaster? ClosureType { get; set; }
        public MiscMaster? ClosureReason { get; set; }
        public MiscMaster? ConvertWonLeadTo { get; set; }
        public ICollection<SalesEnquiryHeader>? SalesEnquiryHeaders { get; set; }
    }
}
