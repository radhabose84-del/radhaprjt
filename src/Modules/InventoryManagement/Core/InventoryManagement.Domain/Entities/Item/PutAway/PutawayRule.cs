using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.Domain.Entities.Item.PutAway
{
    public sealed class PutAwayRule : BaseEntity
    {        
        public int UnitId { get; set; }   
        public int ItemGroupId { get; set; }    
        public ItemGroup ItemGroup { get; set; } = null!;     
        public int ItemCategoryId { get; set; }    
        public ItemCategory ItemCategory { get; set; } = null!;  
        public int? ItemId { get; set; }
        public ItemMaster ItemMaster { get; set; } = null!;  
        public int WarehouseId { get; set; }                       
        public ICollection<PutAwayStrategy> Strategies { get; set; } = new List<PutAwayStrategy>();
    }
}