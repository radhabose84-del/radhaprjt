using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Entities
{
    public class WarehouseItemGroupMapping : BaseEntity
    {
        // FK to Warehouse
        public int WarehouseId { get; set; }

        // Soft FK (ItemGroup from Inventory Service)
        public int ItemGroupId { get; set; }

        // Navigation property
        public WarehouseMaster Warehouse { get; set; } = null!;
    }
}   