using MaintenanceManagement.Application.MachineMaster.Command.CreateMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.UpdateMachineMaster;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.TestData
{
    public static class MachineMasterBuilders
    {
        public static CreateMachineMasterCommand ValidCreateCommand(
            string code = "MCH001",
            string name = "Lathe Machine") =>
            new CreateMachineMasterCommand
            {
                MachineCode = code,
                MachineName = name,
                MachineGroupId = 1,
                UnitId = 1
            };

        public static UpdateMachineMasterCommand ValidUpdateCommand(int id = 1) =>
            new UpdateMachineMasterCommand
            {
                Id = id,
                MachineName = "Updated Lathe Machine",
                MachineGroupId = 1,
                UnitId = 1
            };

        public static DeleteMachineMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMachineMasterCommand { Id = id };

        public static MaintenanceManagement.Domain.Entities.MachineMaster ValidEntity(int id = 1) =>
            new MaintenanceManagement.Domain.Entities.MachineMaster
            {
                Id = id,
                MachineCode = "MCH001",
                MachineName = "Lathe Machine",
                MachineGroupId = 1,
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
