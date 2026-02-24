#nullable disable

namespace SalesManagement.Application.SalesGroup.Dto
{
    public class SalesGroupDto
    {
        public int Id { get; set; }
        public string SalesGroupName { get; set; }
        public int SalesOfficeId { get; set; }
        public string SalesOfficeName { get; set; }
        public string ResponsibleManager { get; set; }
        public int? ProductCategoryId { get; set; }
        public string ProductCategoryName { get; set; }
        public string RegionTerritory { get; set; }
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
