using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Entities
{
    public class WarehouseMaster : BaseEntity
    {
        public string WarehouseCode { get; set; } = null!;
        public string WarehouseName { get; set; } = null!;
        public int UnitId { get; set; }
        public int? ParentWarehouseId { get; set; }
        public bool IsGroup { get; set; }
        public bool IsVirtualWarehouse { get; set; }
        public int WarehouseTypeId { get; set; }
        public int DepartmentId { get; set; }
        public int StorageTypeId { get; set; }
        public int AreaTypeId { get; set; }
        public int OperationTypeId { get; set; }
        public int CapacityUOMId { get; set; }
        public int? AccountId { get; set; }  
        public string? ContactPersonName { get; set; }
        public string? MobileNumber { get; set; }
        public string? Email { get; set; }
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string Pincode { get; set; } = null!;
        public bool IsScrapWarehouse { get; set; }
        public bool IsTransitWarehouse { get; set; }
        public decimal MaxCapacity { get; set; }
        public bool IsDefaultStockEntry { get; set; }

        // Navigation properties
        public WarehouseMaster? ParentWarehouse { get; set; }
        public ICollection<WarehouseMaster> ChildWarehouses { get; set; } = new List<WarehouseMaster>();

        // Allowed Item Groups mapping
        public ICollection<WarehouseItemGroupMapping> AllowedItemGroups { get; set; } = new List<WarehouseItemGroupMapping>();

        public ICollection<RackMaster> Racks { get; set; } = new List<RackMaster>();
        
        public ICollection<BinMaster> Bins { get; set; } = new List<BinMaster>();
    }
}