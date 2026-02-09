using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace WarehouseManagement.Application.WarehouseMaster.Command.CreateWarehouseMaster
{
    public class CreateWarehouseMasterCommand : IRequest<int> // returning new warehouse ID
    {
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

        public string City { get; set; } = null!;
        public string State { get; set; } = null!;
        public string Country { get; set; } = null!;  
            
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string Pincode { get; set; } = null!;
        public bool IsScrapWarehouse { get; set; }
        public bool IsTransitWarehouse { get; set; }
        public decimal MaxCapacity { get; set; }
        public bool IsDefaultStockEntry { get; set; }
        public List<int> AllowedItemGroupIds { get; set; } = new();
        
    }
}