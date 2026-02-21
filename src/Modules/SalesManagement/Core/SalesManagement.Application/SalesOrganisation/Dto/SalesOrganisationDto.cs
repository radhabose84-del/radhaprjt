#nullable disable
namespace SalesManagement.Application.SalesOrganisation.Dto
{
    public class SalesOrganisationDto
    {
        public int Id { get; set; }
        public string SalesOrganisationCode { get; set; }
        public string SalesOrganisationName { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string ModifiedByName { get; set; }
        public string ModifiedIP { get; set; }
    }
}
