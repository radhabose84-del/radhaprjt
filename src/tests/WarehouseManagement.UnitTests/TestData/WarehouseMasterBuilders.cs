using WarehouseManagement.Application.WarehouseMaster.Command.CreateWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.UpdateWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.GetAllWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Queries.GetWareMasterAutoComplete;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.UnitTests.TestData
{
    public static class WarehouseMasterBuilders
    {
        public static CreateWarehouseMasterCommand ValidCreateCommand(
            string name = "Test Warehouse",
            int unitId = 1,
            int warehouseTypeId = 1,
            int storageTypeId = 1,
            int capacityUOMId = 1,
            decimal maxCapacity = 100m) =>
            new CreateWarehouseMasterCommand
            {
                WarehouseName = name,
                UnitId = unitId,
                WarehouseTypeId = warehouseTypeId,
                DepartmentId = 1,
                StorageTypeId = storageTypeId,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = capacityUOMId,
                AddressLine1 = "123 Main St",
                City = "Mumbai",
                State = "Maharashtra",
                Country = "India",
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                Pincode = "400001",
                MaxCapacity = maxCapacity,
                AllowedItemGroupIds = new List<int> { 1, 2 }
            };

        public static UpdateWarehouseMasterCommand ValidUpdateCommand(
            int id = 1,
            string? name = "Updated Warehouse",
            int unitId = 1,
            int warehouseTypeId = 1,
            byte isActive = 1) =>
            new UpdateWarehouseMasterCommand
            {
                Id = id,
                WarehouseName = name,
                UnitId = unitId,
                WarehouseTypeId = warehouseTypeId,
                DepartmentId = 1,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = 1,
                AddressLine1 = "123 Main St",
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                Pincode = "400001",
                MaxCapacity = 100,
                IsActive = isActive,
                AllowedItemGroupIds = new List<int> { 1, 2 }
            };

        public static WarehouseMasterDto ValidDto(
            int id = 1,
            string code = "WH001",
            string name = "Test Warehouse") =>
            new WarehouseMasterDto
            {
                Id = id,
                WarehouseCode = code,
                WarehouseName = name,
                UnitId = 1,
                WarehouseTypeId = 1,
                DepartmentId = 1,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = 1,
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                Pincode = "400001",
                AddressLine1 = "123 Main St",
                MaxCapacity = 100m,
                IsActive = 1
            };

        public static List<GetWarehouseAutoCompleteDto> ValidAutoCompleteList() =>
            new List<GetWarehouseAutoCompleteDto>
            {
                new GetWarehouseAutoCompleteDto { Id = 1, WarehouseCode = "WH001", WarehouseName = "Test Warehouse" }
            };

        public static WarehouseMaster ValidEntity(int id = 1) =>
            new WarehouseMaster
            {
                Id = id,
                WarehouseCode = "WH001",
                WarehouseName = "Test Warehouse",
                UnitId = 1,
                WarehouseTypeId = 1,
                DepartmentId = 1,
                StorageTypeId = 1,
                AreaTypeId = 1,
                OperationTypeId = 1,
                CapacityUOMId = 1,
                AddressLine1 = "123 Main St",
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                Pincode = "400001",
                MaxCapacity = 100m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
    }
}
