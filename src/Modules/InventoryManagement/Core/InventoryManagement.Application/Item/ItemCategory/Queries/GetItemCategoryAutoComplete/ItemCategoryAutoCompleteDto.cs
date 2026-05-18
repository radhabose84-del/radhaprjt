using Contracts.Dtos.Lookups.Users;

namespace InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete
{
    public class ItemCategoryAutoCompleteDto
    {
        public int Id { get; set; }
        public string? ItemCategoryName { get; set; }
        public string? ParentCategoryName { get; set; }
        public int? ItemGroupId { get; set; }
        public string? ItemGroupName { get; set; }
        public List<ModuleLookupDto> Modules { get; set; } = new();
        public int? EmergencyPOById { get; set; }
        public string? EmergencyPOByName { get; set; }
        public decimal? EmergencyValueLimit { get; set; }
        public int? EmergencyActionId { get; set; }
        public string? EmergencyActionName { get; set; }
    }
}