using Contracts.Common;
using MaintenanceManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using MaintenanceManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.TestData
{
    public static class MiscTypeMasterMMBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string code = "MT001",
            string description = "Test Misc Type") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = code,
                Description = description
            };

        public static DeleteMiscTypeMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscTypeMasterCommand { Id = id };

        public static GetMiscTypeMasterDto ValidDto(int id = 1) =>
            new GetMiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = "MT001",
                Description = "Test Misc Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static MaintenanceManagement.Domain.Entities.MiscTypeMaster ValidEntity(int id = 1) =>
            new MaintenanceManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = "MT001",
                Description = "Test Misc Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
