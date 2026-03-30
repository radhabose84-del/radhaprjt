using FAM.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using FAM.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class MiscTypeMasterBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string miscTypeCode = "MTYPE001",
            string description = "TestMiscType") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = miscTypeCode,
                Description = description
            };

        public static UpdateMiscTypeMasterCommand ValidUpdateCommand(
            int id = 1,
            string miscTypeCode = "MTYPE001",
            string description = "UpdatedMiscType",
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

        public static GetMiscTypeMasterDto ValidDto(int id = 1) =>
            new GetMiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = "MTYPE001",
                Description = "TestMiscType",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static GetMiscTypeMasterAutocompleteDto ValidAutoCompleteDto(int id = 1) =>
            new GetMiscTypeMasterAutocompleteDto
            {
                Id = id,
                MiscTypeCode = "MTYPE001"
            };

        public static FAM.Domain.Entities.MiscTypeMaster ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = "MTYPE001",
                Description = "TestMiscType",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
