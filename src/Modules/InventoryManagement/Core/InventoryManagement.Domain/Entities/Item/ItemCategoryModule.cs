namespace InventoryManagement.Domain.Entities.Item
{
    public class ItemCategoryModule
    {
        public int Id { get; set; }
        public int ItemCategoryId { get; set; }
        public ItemCategory ItemCategory { get; set; } = null!;
        public int ModuleId { get; set; }
    }
}
