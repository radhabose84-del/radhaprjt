using WarehouseManagement.Application.RackMaster.Command.CreateRackMaster;
using WarehouseManagement.Application.RackMaster.Command.UpdateRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetAllRackMaster;
using WarehouseManagement.Application.RackMaster.Queries.GetRackMasterAutoComplete;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.UnitTests.TestData
{
    public static class RackMasterBuilders
    {
        public static CreateRackMasterCommand ValidCreateCommand(
            int warehouseId = 1,
            string? rackName = "Rack A",
            int? floorId = 1,
            int? aisleId = 1,
            int? rackLevelId = 1) =>
            new CreateRackMasterCommand
            {
                WarehouseId = warehouseId,
                RackName = rackName,
                FloorId = floorId,
                AisleId = aisleId,
                RackLevelId = rackLevelId,
                MaxCapacity = 500m,
                CapacityUOMId = 1,
                RackWidth = 2.5m,
                RackHeight = 3.0m,
                DimensionUOMId = 1
            };

        public static UpdateRackMasterCommand ValidUpdateCommand(
            int id = 1,
            int warehouseId = 1,
            byte isActive = 1) =>
            new UpdateRackMasterCommand
            {
                Id = id,
                WarehouseId = warehouseId,
                RackName = "Updated Rack",
                FloorId = 1,
                AisleId = 1,
                RackLevelId = 1,
                MaxCapacity = 600m,
                CapacityUOMId = 1,
                RackWidth = 3.0m,
                RackHeight = 3.5m,
                DimensionUOMId = 1,
                IsActive = isActive
            };

        public static RackMasterDto ValidDto(int id = 1, string code = "RK-WH1-F1-A1-L1") =>
            new RackMasterDto
            {
                Id = id,
                WarehouseId = 1,
                RackCode = code,
                RackName = "Rack A",
                FloorId = 1,
                AisleId = 1,
                RackLevelId = 1,
                MaxCapacity = 500m,
                CapacityUOMId = 1,
                RackWidth = 2.5m,
                RackHeight = 3.0m,
                DimensionUOMId = 1,
                IsActive = true
            };

        public static List<GetRackMasterAutoCompleteDto> ValidAutoCompleteList() =>
            new List<GetRackMasterAutoCompleteDto>
            {
                new GetRackMasterAutoCompleteDto { Id = 1, RackCode = "RK-WH1-F1-A1-L1", RackName = "Rack A" }
            };

        public static RackMaster ValidEntity(int id = 1) =>
            new RackMaster
            {
                Id = id,
                WarehouseId = 1,
                RackCode = "RK-WH1-F1-A1-L1",
                RackName = "Rack A",
                FloorId = 1,
                AisleId = 1,
                RackLevelId = 1,
                MaxCapacity = 500m,
                CapacityUOMId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
    }
}
