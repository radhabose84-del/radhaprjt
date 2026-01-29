// Contracts/Dtos/Warehouse/WarehouseDto.cs
namespace Contracts.Dtos.Warehouse
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }

        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;

        public int WarehouseTypeId { get; set; }
        public int StorageTypeId { get; set; }
        public int AreaTypeId { get; set; }
        public int OperationTypeId { get; set; }

        public double Capacity { get; set; }
        public int CapacityUOMId { get; set; }

        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }  
        public int ParentWarehouseId { get; set; }     
    }
}
