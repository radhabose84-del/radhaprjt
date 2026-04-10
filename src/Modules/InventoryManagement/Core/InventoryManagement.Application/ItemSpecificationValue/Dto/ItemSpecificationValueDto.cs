
namespace InventoryManagement.Application.ItemSpecificationValue.Dto
{
    public class ItemSpecificationValueDto
    {
        public int Id { get; set; }
        public int SpecificationMasterId { get; set; }
        public string? SpecificationMasterName { get; set; }
        public string? SpecificationValue { get; set; }
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
