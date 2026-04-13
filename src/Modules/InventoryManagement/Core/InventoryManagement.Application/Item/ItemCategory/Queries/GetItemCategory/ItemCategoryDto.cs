// InventoryManagement.Application/Item/ItemCategory/Queries/GetItemCategory/ItemCategoryDto.cs
using System.Text.Json.Serialization;
using Contracts.Dtos.Lookups.Users;
using InventoryManagement.Application.Common.Mappings;

namespace InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory
{
    public class ItemCategoryDto : IMapFrom<Domain.Entities.Item.ItemCategory>
    {
        public int Id { get; set; }
        public string? ItemCategoryName { get; set; }
        public int ItemGroupId { get; set; }
        public string? ItemGroupName { get; set; }
        public int IsGroup { get; set; }                 // keep your int flags as-is
        public int? ParentCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public int IsBudgetApplicable { get; set; }
        public List<ModuleLookupDto> Modules { get; set; } = new();
        public int IsActive { get; set; }
        public int IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }

        // from the CTE; used to cap depth when stitching
        public int Level { get; set; }

        // Downward-only link (safe for JSON)
        public List<ItemCategoryDto> SubGroups { get; set; } = new();

        // Optional upward link for internal use only (ignored in JSON)
        [JsonIgnore] public ItemCategoryDto? Parent { get; set; }
    }
}
