using MaintenanceManagement.Application.MaintenanceType.Command.CreateMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType;
using MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.TestData
{
    public static class MaintenanceTypeBuilders
    {
        public static CreateMaintenanceTypeCommand ValidCreateCommand(
            string typeName = "Preventive") =>
            new CreateMaintenanceTypeCommand
            {
                TypeName = typeName
            };

        public static UpdateMaintenanceTypeCommand ValidUpdateCommand(
            int id = 1,
            string typeName = "Corrective",
            byte isActive = 1) =>
            new UpdateMaintenanceTypeCommand
            {
                Id = id,
                TypeName = typeName,
                IsActive = isActive
            };

        public static DeleteMaintenanceTypeCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMaintenanceTypeCommand { Id = id };

        public static MaintenanceTypeDto ValidDto(int id = 1) =>
            new MaintenanceTypeDto
            {
                Id = id,
                TypeName = "Preventive",
                IsActive = Status.Active,
                CreatedDate = DateTimeOffset.UtcNow
            };

        public static MaintenanceTypeAutoCompleteDto ValidAutoCompleteDto(int id = 1) =>
            new MaintenanceTypeAutoCompleteDto
            {
                Id = id,
                TypeName = "Preventive"
            };

        public static MaintenanceManagement.Domain.Entities.MaintenanceType ValidEntity(int id = 1) =>
            new MaintenanceManagement.Domain.Entities.MaintenanceType
            {
                Id = id,
                TypeName = "Preventive",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
