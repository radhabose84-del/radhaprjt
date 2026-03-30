using MaintenanceManagement.Application.MiscMaster.Command.CreateMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using MaintenanceManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.TestData
{
    public static class MiscMasterMMBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            string code = "MC001",
            string description = "Test Misc") =>
            new CreateMiscMasterCommand
            {
                MiscTypeId = 1,
                Code = code,
                Description = description
            };

        public static DeleteMiscMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscMasterCommand { Id = id };

        public static GetMiscMasterDto ValidDto(int id = 1) =>
            new GetMiscMasterDto
            {
                Id = id,
                MiscTypeId = 1,
                Code = "MC001",
                Description = "Test Misc",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static MaintenanceManagement.Domain.Entities.MiscMaster ValidEntity(int id = 1) =>
            new MaintenanceManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                MiscTypeId = 1,
                Code = "MC001",
                Description = "Test Misc",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
