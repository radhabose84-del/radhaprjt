#nullable disable
using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesOffice : BaseEntity
    {
        public string SalesOfficeName { get; set; }
        public int SalesOrganisationId { get; set; }
        public int CityId { get; set; }
        public string Pincode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ResponsibleManager { get; set; }
        public string RegionTerritory { get; set; }
        public string Address { get; set; }

        // Navigation property
        public SalesOrganisation SalesOrganisation { get; set; }
    }
}
