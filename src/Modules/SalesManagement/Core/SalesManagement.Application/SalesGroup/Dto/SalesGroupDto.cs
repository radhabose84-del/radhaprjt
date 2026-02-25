
namespace SalesManagement.Application.SalesGroup.Dto
{
    public class SalesGroupDto
    {
        public int Id { get; set; }
        public string SalesGroupName { get; set; } = null!;
        public int SalesOfficeId { get; set; }
        public string SalesOfficeName { get; set; } = null!;
        public string? ResponsibleManager { get; set; }
        public int? ProductCategoryId { get; set; }
        public string? ProductCategoryName { get; set; }
        public string? RegionTerritory { get; set; }
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
