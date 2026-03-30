using ProjectManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using ProjectManagement.Domain.Common;
using static ProjectManagement.Domain.Common.BaseEntity;

namespace ProjectManagement.UnitTests.TestData
{
    public static class MiscTypeMasterBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string? miscTypeCode = "MTT001",
            string? description = "Test MiscType") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = miscTypeCode,
                Description = description
            };

        public static UpdateMiscTypeMasterCommand ValidUpdateCommand(
            int id = 1,
            string? miscTypeCode = "MTT001",
            string? description = "Updated MiscType",
            byte isActive = 1) =>
            new UpdateMiscTypeMasterCommand
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = isActive
            };

        public static DeleteMiscTypeMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscTypeMasterCommand { Id = id };

        public static GetMiscTypeMasterDto ValidDto(
            int id = 1,
            string? miscTypeCode = "MTT001",
            string? description = "Test MiscType") =>
            new GetMiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static ProjectManagement.Domain.Entities.MiscTypeMaster ValidEntity(
            int id = 1,
            string? miscTypeCode = "MTT001",
            string? description = "Test MiscType") =>
            new ProjectManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static GetMiscTypeMasterAutocompleteDto ValidAutocompleteDto(
            int id = 1,
            string? miscTypeCode = "MTT001") =>
            new GetMiscTypeMasterAutocompleteDto
            {
                Id = id,
                MiscTypeCode = miscTypeCode
            };
    }
}
