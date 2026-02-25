
namespace SalesManagement.Application.SalesOffice.Dto
{
    public class SalesOfficeDto
    {
        public int Id { get; set; }
        public string SalesOfficeName { get; set; } = null!;
        public int SalesOrganisationId { get; set; }
        public string SalesOrganisationName { get; set; } = null!;
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public string Pincode { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string ResponsibleManager { get; set; } = null!;
        public string RegionTerritory { get; set; } = null!;
        public string Address { get; set; } = null!;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string CreatedByName { get; set; } = null!;
        public string CreatedIP { get; set; } = null!;
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string ModifiedByName { get; set; } = null!;
        public string ModifiedIP { get; set; } = null!;
    }
}
