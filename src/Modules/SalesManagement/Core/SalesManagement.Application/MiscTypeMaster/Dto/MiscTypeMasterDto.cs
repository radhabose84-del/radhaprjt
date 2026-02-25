namespace SalesManagement.Application.MiscTypeMaster.Dto
{
    public class MiscTypeMasterDto
    {
        public int Id { get; set; }
        public string MiscTypeCode { get; set; } = null!;
        public string Description { get; set; } = null!;
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
