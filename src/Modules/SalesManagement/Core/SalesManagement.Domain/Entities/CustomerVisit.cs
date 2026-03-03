using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class CustomerVisit : BaseEntity
    {
        // Customer (cross-module FK → PartyManagement, no DB constraint)
        public int CustomerId { get; set; }

        // Visit details
        public int VisitTypeId { get; set; }                // Same-module FK → Sales.MiscMaster
        public DateTimeOffset VisitDateTime { get; set; }

        // Geo-location
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        // Photo
        public string? ImageName { get; set; }

        // Notes
        public string? Remarks { get; set; }

        // Ownership (same-module FK → Sales.MarketingOfficer)
        public int MarketingOfficerId { get; set; }

        // Same-module navigation properties
        public MiscMaster? VisitType { get; set; }
        public MarketingOfficer? MarketingOfficer { get; set; }

        // Child collection
        public ICollection<CustomerVisitProduct>? CustomerVisitProducts { get; set; }
    }
}
