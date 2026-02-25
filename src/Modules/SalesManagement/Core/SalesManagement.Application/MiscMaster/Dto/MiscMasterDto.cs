namespace SalesManagement.Application.MiscMaster.Dto
{
    public class MiscMasterDto
    {
        public int Id { get; set; }
        public int MiscTypeId { get; set; }
        public string MiscTypeCode { get; set; } = null!;
        public string MiscTypeDescription { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int SortOrder { get; set; }
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
