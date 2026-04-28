namespace SalesManagement.Application.SalesLead.Dto
{
    public class SalesLeadDto
    {
        public int Id { get; set; }

        // Party identification
        public int? PartyId { get; set; }
        public string? PartyName { get; set; }          // populated via IPartyLookup
        public string? ProspectCompanyName { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }           // populated via ICityLookup

        // Contact person
        public string? ContactName { get; set; }
        public string? MobileNumber { get; set; }
        public string? EmailId { get; set; }
        public int? ContactId { get; set; }
        public string? ExistingContactName { get; set; } // populated via SQL JOIN → SalesContact

        // Requirement snapshot
        public int? ItemId { get; set; }
        public string? ItemName { get; set; }           // populated via IItemLookup
        public int? VariantId { get; set; }
        public string? VariantName { get; set; }       // populated via IItemLookup
        public decimal? RequirementQty { get; set; }
        public DateOnly? ExpectedDate { get; set; }
        public string? Remarks { get; set; }

        // Ownership & source
        public int? LeadSourceId { get; set; }
        public string? LeadSourceName { get; set; }     // populated via SQL JOIN → MiscMaster
        public int MarketingOfficerId { get; set; }
        public string? MarketingOfficerName { get; set; } // populated via SQL JOIN → MarketingOfficer
        public DateTimeOffset InteractionDate { get; set; }

        // Enquiry linkage
        public bool IsEditable { get; set; }           // false when enquiry exists against this lead
        public string? EnquiryNo { get; set; }         // populated via OUTER APPLY → SalesEnquiryHeader

        // Status
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Audit fields
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
