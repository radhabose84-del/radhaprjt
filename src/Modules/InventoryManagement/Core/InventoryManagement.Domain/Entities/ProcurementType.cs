using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.Domain.Entities
{
    public class ProcurementType : BaseEntity
    {
        public string? ProcurementCode { get; set; }
        public string? ProcurementName { get; set; }
        public ICollection<ItemUnitMapping>? ItemUnitMappings { get; set; }
    }
}
