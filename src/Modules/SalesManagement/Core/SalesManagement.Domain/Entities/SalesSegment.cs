
using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesSegment : BaseEntity
    {
        // Foreign Key Fields (Form Sales Area)
        public int SalesOrganisationId { get; set; }
        public int SalesChannelId { get; set; }
        public int BusinessUnitId { get; set; }
        public int? CurrencyId { get; set; }  // Cross-module FK (no navigation)

        // Navigation Properties (Same-Module FKs)
        public SalesOrganisation? SalesOrganisation { get; set; }
        public SalesChannel? SalesChannel { get; set; }
        public BusinessUnit? BusinessUnit { get; set; }

        // Other Properties
        public DateTime? ValidFrom { get; set; }
        public string? SegmentName { get; set; }

        // Reverse navigation
        public ICollection<SalesOrderHeader>? SalesOrderHeaders { get; set; }
    }
}
