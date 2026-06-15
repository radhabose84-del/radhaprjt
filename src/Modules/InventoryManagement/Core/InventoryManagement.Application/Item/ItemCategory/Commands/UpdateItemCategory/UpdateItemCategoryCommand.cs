using InventoryManagement.Application.Item.ItemCategory.Commands.Shared;
using MediatR;
using Contracts.Common;

namespace InventoryManagement.Application.Item.ItemCategory.Commands.UpdateItemCategory
{
    public class UpdateItemCategoryCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public int ItemGroupId { get; set; }
        public string? ItemCategoryName { get; set; }
        public byte? IsGroup { get; set; }
        public int? ParentCategoryId { get; set; }
        public byte IsBudgetApplicable { get; set; }
        public int? EmergencyPOById { get; set; }
        public decimal? EmergencyValueLimit { get; set; }
        public int? EmergencyActionId { get; set; }
        public byte IsActive { get; set; }
        public List<int> ModuleIds { get; set; } = new();
        public List<SampleQuantityItem> SampleQuantities { get; set; } = new();
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
