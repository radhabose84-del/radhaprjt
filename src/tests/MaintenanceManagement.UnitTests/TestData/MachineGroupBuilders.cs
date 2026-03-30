using MaintenanceManagement.Application.MachineGroup.Command.CreateMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.DeleteMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Command.UpdateMachineGroup;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.TestData
{
    public static class MachineGroupBuilders
    {
        public static CreateMachineGroupCommand ValidCreateCommand(string groupName = "Lathe Group") =>
            new CreateMachineGroupCommand { GroupName = groupName, UnitId = 1 };

        public static UpdateMachineGroupCommand ValidUpdateCommand(int id = 1) =>
            new UpdateMachineGroupCommand { Id = id, GroupName = "Updated Lathe Group", UnitId = 1 };

        public static DeleteMachineGroupCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMachineGroupCommand { Id = id };

        public static MaintenanceManagement.Domain.Entities.MachineGroup ValidEntity(int id = 1) =>
            new MaintenanceManagement.Domain.Entities.MachineGroup
            {
                Id = id,
                GroupName = "Lathe Group",
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
