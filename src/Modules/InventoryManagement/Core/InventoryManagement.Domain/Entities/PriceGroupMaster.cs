using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities
{
    public class PriceGroupMaster : BaseEntity
    {
        public string? PriceGroupCode { get; set; }
        public string? PriceGroupName { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
    }
}
