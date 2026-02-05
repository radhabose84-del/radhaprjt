
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;

namespace InventoryManagement.Domain.Entities.item.ItemDetail.Templates
{
    public class InspectionParameter : BaseEntity
    {
        public int TemplateId { get; set; }
        public InspectionTemplate Template { get; set; } = null!;
        public string Parameter { get; set; } = null!;
        public string? AcceptanceCriteriaValue { get; set; } 
        public bool Numeric { get; set; }
        public decimal? MinimumValue { get; set; }
        public decimal? MaximumValue { get; set; }
    }
}