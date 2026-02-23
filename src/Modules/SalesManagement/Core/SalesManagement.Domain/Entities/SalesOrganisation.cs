#nullable disable
using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesOrganisation : BaseEntity
    {
        public string SalesOrganisationCode { get; set; }
        public string SalesOrganisationName { get; set; }
        public int CompanyId { get; set; }
        public string Description { get; set; }

        // Navigation properties for reverse relationships
        public ICollection<SalesSegment> SalesSegments { get; set; }
        public ICollection<SalesOffice> SalesOffices { get; set; }
    }
}
