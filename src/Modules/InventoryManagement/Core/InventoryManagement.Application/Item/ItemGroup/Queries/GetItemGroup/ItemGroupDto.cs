using InventoryManagement.Application.Common.Mappings;

namespace InventoryManagement.Application.Item.ItemGroup.Queries.GetItemGroup
    {
    public class ItemGroupDto : IMapFrom<Domain.Entities.Item.ItemGroup>
    {
        public int Id { get; set; }
        public string? ItemGroupName { get; set; }
        public string? ItemGroupCode { get; set; }
        public int UnitId { get; set; }
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
        }      
    }