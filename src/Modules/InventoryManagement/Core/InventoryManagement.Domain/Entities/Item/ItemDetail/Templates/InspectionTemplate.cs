

using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.item.ItemDetail.Templates;

namespace InventoryManagement.Domain.Entities.Item.ItemDetail.Templates
{
    public class InspectionTemplate : BaseEntity
    {
        public string TemplateName { get; set; } = null!;
        public ICollection<InspectionParameter> Parameters { get; set; } = new List<InspectionParameter>();        
        public ICollection<ItemQuality> Items { get; set; } = new List<ItemQuality>();
    }
}