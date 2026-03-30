using ProjectManagement.Application.MiscMaster.Command.CreateMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using ProjectManagement.Application.MiscMaster.Queries.GetMiscMaster;
using ProjectManagement.Domain.Common;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.UnitTests.TestData
{
    public static class MiscMasterBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            int miscTypeId = 1,
            string? code = "MSC001",
            string? description = "Test MiscMaster") =>
            new CreateMiscMasterCommand
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description
            };

        public static UpdateMiscMasterCommand ValidUpdateCommand(
            int id = 1,
            int miscTypeId = 1,
            string? code = "MSC001",
            string? description = "Updated MiscMaster",
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
            string? code = "MSC001",
            string? description = "Test MiscMaster") =>
            new GetMiscMasterDto
            {
                Id = id,
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static ProjectManagement.Domain.Entities.MiscMaster ValidEntity(
            int id = 1,
            int miscTypeId = 1,
            string? code = "MSC001",
            string? description = "Test MiscMaster") =>
            new ProjectManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static GetMiscMasterAutoCompleteDto ValidAutocompleteDto(
            int id = 1,
            string? code = "MSC001",
            string? description = "Test MiscMaster") =>
            new GetMiscMasterAutoCompleteDto
            {
                Id = id,
                Code = code,
                Description = description
            };
    }
}
