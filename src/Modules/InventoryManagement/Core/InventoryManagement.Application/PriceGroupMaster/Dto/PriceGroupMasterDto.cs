namespace InventoryManagement.Application.PriceGroupMaster.Dto
{
    public class PriceGroupMasterDto
    {
        public int Id { get; set; }
        public string? PriceGroupCode { get; set; }
        public string? PriceGroupName { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
