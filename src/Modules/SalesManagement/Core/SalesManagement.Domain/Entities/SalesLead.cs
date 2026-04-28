using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesLead : BaseEntity
    {
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
        public decimal? RequirementQty { get; set; }
        public DateOnly? ExpectedDate { get; set; }
        public string? Remarks { get; set; }

        // Ownership & source
        public int? LeadSourceId { get; set; }          // same-module FK → Sales.MiscMaster
        public int MarketingOfficerId { get; set; }     // same-module FK → Sales.MarketingOfficer
        public DateTimeOffset InteractionDate { get; set; }

        // Same-module navigation properties
        public SalesContact? Contact { get; set; }
        public MiscMaster? LeadSource { get; set; }
        public MarketingOfficer? MarketingOfficer { get; set; }
        public ICollection<SalesEnquiryHeader>? SalesEnquiryHeaders { get; set; }
    }
}
