using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.Domain.Entities
{
    public class UsageType : BaseEntity
    {
        public string? UsageTypeCode { get; set; }
        public string? UsageTypeName { get; set; }
        public string? Description { get; set; }
        public int ModuleId { get; set; }
        public ICollection<ItemUsageTypeMapping>? ItemUsageTypeMappings { get; set; }
    }
}
