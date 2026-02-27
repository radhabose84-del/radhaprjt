
namespace SalesManagement.Application.SalesOffice.Dto
{
    public class SalesOfficeDto
    {
        public int Id { get; set; }
        public string? SalesOfficeName { get; set; }
        public int SalesOrganisationId { get; set; }
        public string? SalesOrganisationName { get; set; }
        public int CityId { get; set; }
        public string? CityName { get; set; }
        public string? Pincode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? ResponsibleManager { get; set; }
        public string? RegionTerritory { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

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
