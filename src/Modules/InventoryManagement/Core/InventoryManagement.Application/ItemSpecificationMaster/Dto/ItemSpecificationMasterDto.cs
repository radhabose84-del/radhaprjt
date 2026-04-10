
namespace InventoryManagement.Application.ItemSpecificationMaster.Dto
{
    public class ItemSpecificationMasterDto
    {
        public int Id { get; set; }
        public string? SpecificationCode { get; set; }
        public string? SpecificationName { get; set; }
        public int Order { get; set; }
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
