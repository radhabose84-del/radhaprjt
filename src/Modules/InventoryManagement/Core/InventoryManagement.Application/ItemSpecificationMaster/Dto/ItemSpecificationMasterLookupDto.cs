
namespace InventoryManagement.Application.ItemSpecificationMaster.Dto
{
    public sealed class ItemSpecificationMasterLookupDto
    {
        public int Id { get; set; }
        public string? SpecificationCode { get; set; }
        public string? SpecificationName { get; set; }
        public int Order { get; set; }
    }
}
