using InventoryManagement.Application.UsageType.Commands.CreateUsageType;
using InventoryManagement.Application.UsageType.Commands.DeleteUsageType;
using InventoryManagement.Application.UsageType.Commands.UpdateUsageType;
using InventoryManagement.Application.UsageType.Dto;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.TestData
{
    public static class UsageTypeBuilders
    {
        public static CreateUsageTypeCommand ValidCreateCommand(
            string code = "UTY001",
            string name = "Test UsageType",
            int moduleId = 1) =>
            new CreateUsageTypeCommand
            {
                UsageTypeCode = code,
                UsageTypeName = name,
                Description = "Test description",
                ModuleId = moduleId
            };

        public static UpdateUsageTypeCommand ValidUpdateCommand(
            int id = 1,
            string name = "Updated UsageType",
            int moduleId = 1,
            int isActive = 1) =>
            new UpdateUsageTypeCommand
            {
                Id = id,
                UsageTypeName = name,
                Description = "Updated description",
                ModuleId = moduleId,
                IsActive = isActive
            };

        public static DeleteUsageTypeCommand ValidDeleteCommand(int id = 1) =>
            new DeleteUsageTypeCommand(id);

        public static UsageTypeDto ValidDto(int id = 1, string code = "UTY001") =>
            new UsageTypeDto
            {
                Id = id,
                UsageTypeCode = code,
                UsageTypeName = "Test UsageType",
                ModuleId = 1,
                IsActive = true,
                IsDeleted = false
            };

        public static UsageTypeLookupDto ValidLookupDto(int id = 1) =>
            new UsageTypeLookupDto
            {
                Id = id,
                UsageTypeCode = "UTY001",
                UsageTypeName = "Test UsageType"
            };

        public static InventoryManagement.Domain.Entities.UsageType ValidEntity(int id = 1) =>
            new InventoryManagement.Domain.Entities.UsageType
            {
                Id = id,
                UsageTypeCode = "UTY001",
                UsageTypeName = "Test UsageType",
                ModuleId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
