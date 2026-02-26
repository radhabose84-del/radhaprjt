using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesOffice : BaseEntity
    {
        public string SalesOfficeName { get; set; } = null!;
        public int SalesOrganisationId { get; set; }
        public int CityId { get; set; }
        public string Pincode { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string ResponsibleManager { get; set; } = null!;
        public string RegionTerritory { get; set; } = null!;
        public string Address { get; set; } = null!;

        // Navigation properties
        public SalesOrganisation SalesOrganisation { get; set; } = null!;
        public ICollection<SalesGroup> SalesGroups { get; set; } = null!;
        public ICollection<MarketingOfficer> MarketingOfficers { get; set; } = null!;
    }
}
