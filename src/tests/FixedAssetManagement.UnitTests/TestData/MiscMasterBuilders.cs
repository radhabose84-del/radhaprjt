using FAM.Application.MiscMaster.Command.CreateMiscMaster;
using FAM.Application.MiscMaster.Command.DeleteMiscMaster;
using FAM.Application.MiscMaster.Command.UpdateMiscMaster;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class MiscMasterBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            int miscTypeId = 1,
            string code = "MISC001",
            string description = "TestMiscMaster") =>
            new CreateMiscMasterCommand
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description
            };

        public static UpdateMiscMasterCommand ValidUpdateCommand(
            int id = 1,
            int miscTypeId = 1,
            string code = "MISC001",
            string description = "UpdatedMiscMaster",
            byte isActive = 1) =>
            new UpdateMiscMasterCommand
            {
                Id = id,
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = isActive
            };

        public static DeleteMiscMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscMasterCommand { Id = id };

        public static GetMiscMasterDto ValidDto(int id = 1) =>
            new GetMiscMasterDto
            {
                Id = id,
                MiscTypeId = 1,
                Code = "MISC001",
                Description = "TestMiscMaster",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                CreatedDate = DateTimeOffset.UtcNow
            };

        public static GetMiscMasterAutoCompleteDto ValidAutoCompleteDto(int id = 1) =>
            new GetMiscMasterAutoCompleteDto
            {
                Id = id,
                Code = "MISC001",
                Description = "TestMiscMaster"
            };

        public static FAM.Domain.Entities.MiscMaster ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.MiscMaster
            {
                Id = id,
                MiscTypeId = 1,
                Code = "MISC001",
                Description = "TestMiscMaster",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
