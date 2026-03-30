using Contracts.Common;
using MaintenanceManagement.Application.WorkCenter.Command.CreateWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.DeleteWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Command.UpdateWorkCenter;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.TestData
{
    public static class WorkCenterBuilders
    {
        public static CreateWorkCenterCommand ValidCreateCommand(
            string code = "WC001",
            string name = "Assembly Work Center") =>
            new CreateWorkCenterCommand
            {
                WorkCenterCode = code,
                WorkCenterName = name,
                UnitId = 1
            };

        public static UpdateWorkCenterCommand ValidUpdateCommand(int id = 1) =>
            new UpdateWorkCenterCommand
            {
                Id = id,
                WorkCenterName = "Updated Work Center",
                UnitId = 1
            };

        public static DeleteWorkCenterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteWorkCenterCommand { Id = id };

        public static MaintenanceManagement.Domain.Entities.WorkCenter ValidEntity(int id = 1) =>
            new MaintenanceManagement.Domain.Entities.WorkCenter
            {
                Id = id,
                WorkCenterCode = "WC001",
                WorkCenterName = "Assembly Work Center",
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
