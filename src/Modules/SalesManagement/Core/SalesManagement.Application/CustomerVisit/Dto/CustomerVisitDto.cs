namespace SalesManagement.Application.CustomerVisit.Dto
{
    public class CustomerVisitDto
    {
        public int Id { get; set; }

        // Customer
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }           // IPartyLookup

        // Visit details
        public int VisitTypeId { get; set; }
        public string? VisitTypeName { get; set; }          // SQL JOIN (MiscMaster.Description)
        public DateTimeOffset VisitDateTime { get; set; }

        // Geo-location
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Photo
        public string? ImageName { get; set; }
        public string? ImagePath { get; set; }              // Full constructed path

        // Notes
        public string? Remarks { get; set; }

        // Ownership
        public int MarketingOfficerId { get; set; }
        public string? MarketingOfficerName { get; set; }   // SQL JOIN (MarketingOfficer)

        // Status
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        // Detail collections
        public List<CustomerVisitProductDto>? Products { get; set; }

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
