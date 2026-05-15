using MediatR;

namespace InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory
{
    public class UpdateItemCategoryCommand : IRequest<int>
    {
        public int Id { get; set; }
        public int ItemGroupId { get; set; }
        public string? ItemCategoryName { get; set; }
        public byte? IsGroup { get; set; }
        public int? ParentCategoryId { get; set; }
        public byte IsBudgetApplicable { get; set; }
        public byte? EmergencyPoApplicable { get; set; }
        public decimal? EmergencyPoLimit { get; set; }
        public byte IsActive { get; set; }
        public List<int> ModuleIds { get; set; } = new();
    }
}