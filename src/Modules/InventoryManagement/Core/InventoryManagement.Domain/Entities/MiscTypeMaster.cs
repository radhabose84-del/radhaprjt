using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Domain.Entities
{
    public class MiscTypeMaster : BaseEntity
    {
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; }
        public ICollection<MiscMaster>? MiscMaster { get; set; }          
        public ICollection<ItemVariantAttribute>? ItemVariantAttributeGroup { get; set; }  
    }
}