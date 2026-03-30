using WarehouseManagement.Application.BinMaster.Command.CreateBinMaster;
using WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster;
using WarehouseManagement.Application.BinMaster.Queries.GetAllBinMaster;
using WarehouseManagement.Application.BinMaster.Queries.GetBinMasterAutoComplete;
using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.UnitTests.TestData
{
    public static class BinMasterBuilders
    {
        public static CreateBinMasterCommand ValidCreateCommand(
            int warehouseId = 1,
            int? rackId = 1,
            decimal binCapacity = 100m,
            int capacityUOMId = 1) =>
            new CreateBinMasterCommand
            {
                WarehouseId = warehouseId,
                RackId = rackId,
                BinName = "Bin A",
                BinCapacity = binCapacity,
                CapacityUOMId = capacityUOMId
            };

        public static UpdateBinMasterCommand ValidUpdateCommand(
            int id = 1,
            byte isActive = 1) =>
            new UpdateBinMasterCommand
            {
                Id = id,
                BinName = "Updated Bin",
                BinCapacity = 200m,
                CapacityUOMId = 1,
                IsActive = isActive,
                RackId = 1
            };

        public static BinMasterDto ValidDto(int id = 1, string code = "BIN-WH1-RK1-001") =>
            new BinMasterDto
            {
                Id = id,
                BinCode = code,
                BinName = "Bin A",
                WarehouseId = 1,
                RackId = 1,
                BinCapacity = 100m,
                CapacityUOMId = 1,
                IsActive = 1
            };

        public static List<BinAutoDto> ValidAutoCompleteList() =>
            new List<BinAutoDto>
            {
                new BinAutoDto { Id = 1, BinCode = "BIN-WH1-RK1-001", BinName = "Bin A" }
            };

        public static BinMaster ValidEntity(int id = 1) =>
            new BinMaster
            {
                Id = id,
                WarehouseId = 1,
                RackId = 1,
                BinCode = "BIN-WH1-RK1-001",
                BinName = "Bin A",
                BinCapacity = 100m,
                CapacityUOMId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
    }
}
