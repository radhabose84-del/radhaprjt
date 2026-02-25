using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesOrganisation : BaseEntity
    {
        public string SalesOrganisationCode { get; set; } = null!;
        public string SalesOrganisationName { get; set; } = null!;
        public int CompanyId { get; set; }
        public string Description { get; set; } = null!;

        // Navigation properties for reverse relationships
        public ICollection<SalesSegment> SalesSegments { get; set; } = null!;
        public ICollection<SalesOffice> SalesOffices { get; set; } = null!;
    }
}
