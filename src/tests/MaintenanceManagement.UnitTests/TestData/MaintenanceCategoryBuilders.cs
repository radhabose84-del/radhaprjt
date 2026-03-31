using MaintenanceManagement.Application.MaintenanceCategory.Command.CreateMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Command.UpdateMaintenanceCategory;
using MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.TestData
{
    public static class MaintenanceCategoryBuilders
    {
        public static CreateMaintenanceCategoryCommand ValidCreateCommand(
            string categoryName = "Electrical",
            string? description = "Electrical maintenance") =>
            new CreateMaintenanceCategoryCommand
            {
                CategoryName = categoryName,
                Description = description
            };

        public static UpdateMaintenanceCategoryCommand ValidUpdateCommand(
            int id = 1,
            string categoryName = "Updated Electrical",
            string? description = "Updated description",
            byte isActive = 1) =>
            new UpdateMaintenanceCategoryCommand
            {
                Id = id,
                CategoryName = categoryName,
                Description = description,
                IsActive = isActive
            };

        public static DeleteMaintenanceCategoryCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMaintenanceCategoryCommand { Id = id };

        public static MaintenanceCategoryDto ValidDto(int id = 1) =>
            new MaintenanceCategoryDto
            {
                Id = id,
                CategoryName = "Electrical",
                Description = "Electrical maintenance",
                IsActive = Status.Active,
                CreatedDate = DateTimeOffset.UtcNow
            };

        public static MaintenanceCategoryAutoCompleteDto ValidAutoCompleteDto(int id = 1) =>
            new MaintenanceCategoryAutoCompleteDto
            {
                Id = id,
                CategoryName = "Electrical"
            };

        public static MaintenanceManagement.Domain.Entities.MaintenanceCategory ValidEntity(int id = 1) =>
            new MaintenanceManagement.Domain.Entities.MaintenanceCategory
            {
                Id = id,
                CategoryName = "Electrical",
                Description = "Electrical maintenance",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
