
namespace InventoryManagement.Application.ItemSpecificationValue.Dto
{
    public sealed class ItemSpecificationValueLookupDto
    {
        public int Id { get; set; }
        public int SpecificationMasterId { get; set; }
        public string? SpecificationValue { get; set; }
    }
}
