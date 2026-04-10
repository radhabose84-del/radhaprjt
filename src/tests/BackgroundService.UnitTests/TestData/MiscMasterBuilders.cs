using BackgroundService.Application.MiscMaster.Command.CreateMiscMaster;
using BackgroundService.Application.MiscMaster.Command.UpdateMiscMaster;
using BackgroundService.Application.MiscMaster.Command.DeleteMiscMaster;
using BackgroundService.Application.MiscMaster.Queries.GetMiscMaster;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.TestData
{
    public static class MiscMasterBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            int miscTypeId = 1,
            string code = "MISC001",
            string description = "Test Misc Master") =>
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
            string description = "Updated Misc Master",
            int sortOrder = 1,
            byte isActive = 1) =>
            new UpdateMiscMasterCommand
            {
                Id = id,
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = isActive
            };

        public static DeleteMiscMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscMasterCommand { Id = id };

        public static GetMiscMasterDto ValidDto(
            int id = 1,
            int miscTypeId = 1,
            string code = "MISC001",
            string description = "Test Misc Master",
            int sortOrder = 1) =>
            new GetMiscMasterDto
            {
                Id = id,
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static GetMiscMasterAutoCompleteDto ValidAutoCompleteDto(
            int id = 1,
            string code = "MISC001",
            string description = "Test Misc Master") =>
            new GetMiscMasterAutoCompleteDto
            {
                Id = id,
                Code = code,
                Description = description
            };

        public static BackgroundService.Domain.Entities.Notification.MiscMaster ValidEntity(
            int id = 1,
            int miscTypeId = 1,
            string code = "MISC001",
            string description = "Test Misc Master",
            int sortOrder = 1) =>
            new BackgroundService.Domain.Entities.Notification.MiscMaster
            {
                Id = id,
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
