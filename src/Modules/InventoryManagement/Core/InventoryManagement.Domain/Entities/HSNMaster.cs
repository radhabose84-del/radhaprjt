using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.Domain.Entities
{
    public class HSNMaster : BaseEntity
    {
        public int TypeId { get; set; }
        public MiscMaster? Type { get; set; }
        public string? HSNCode { get; set; }
        public string? Description { get; set; }
        public int GSTCategoryId { get; set; }
        public MiscMaster? GstCategory { get; set; }
        private decimal _gstPercentage;
        public decimal GSTPercentage
        {
            get => _gstPercentage;
            set
            {
                _gstPercentage = value;
                CGSTPercentage = Math.Round(value / 2, 2);
                SGSTPercentage = Math.Round(value / 2, 2);
            }
        }
        public decimal CGSTPercentage { get; private set; }
        public decimal SGSTPercentage { get; private set; }
        public decimal IGSTPercentage { get; set; }
        public DateTimeOffset ValidFrom { get; set; } 
        public ICollection<ItemMaster>? ItemMasterHSN { get; set; } 
    }
}