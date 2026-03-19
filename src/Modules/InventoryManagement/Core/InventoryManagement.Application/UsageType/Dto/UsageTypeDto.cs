namespace InventoryManagement.Application.UsageType.Dto
{
    public class UsageTypeDto
    {
        public int Id { get; set; }
        public string? UsageTypeCode { get; set; }
        public string? UsageTypeName { get; set; }
        public string? Description { get; set; }
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
