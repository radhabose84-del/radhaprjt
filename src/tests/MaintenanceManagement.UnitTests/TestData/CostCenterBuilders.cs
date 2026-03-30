using MaintenanceManagement.Application.CostCenter.Command.CreateCostCenter;
using MaintenanceManagement.Application.CostCenter.Command.DeleteCostCenter;
using MaintenanceManagement.Application.CostCenter.Command.UpdateCostCenter;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.TestData
{
    public static class CostCenterBuilders
    {
        public static CreateCostCenterCommand ValidCreateCommand(
            string code = "CC001",
            string name = "Production Cost Center") =>
            new CreateCostCenterCommand
            {
                CostCenterCode = code,
                CostCenterName = name,
                UnitId = 1
            };

        public static UpdateCostCenterCommand ValidUpdateCommand(int id = 1) =>
            new UpdateCostCenterCommand
            {
                Id = id,
                CostCenterName = "Updated Cost Center",
                UnitId = 1
            };

        public static DeleteCostCenterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteCostCenterCommand { Id = id };

        public static MaintenanceManagement.Domain.Entities.CostCenter ValidEntity(int id = 1) =>
            new MaintenanceManagement.Domain.Entities.CostCenter
            {
                Id = id,
                CostCenterCode = "CC001",
                CostCenterName = "Production Cost Center",
                UnitId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
