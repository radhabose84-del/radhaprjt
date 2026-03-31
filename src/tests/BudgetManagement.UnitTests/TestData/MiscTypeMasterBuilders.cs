using BudgetManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using BudgetManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.TestData
{
    public static class MiscTypeMasterBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string? miscTypeCode = "MTY001",
            string? description = "Test MiscType") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = miscTypeCode,
                Description = description
            };

        public static UpdateMiscTypeMasterCommand ValidUpdateCommand(
            int id = 1,
            string? miscTypeCode = "MTY001",
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
            string? miscTypeCode = "MTY001",
            string? description = "Test MiscType") =>
            new GetMiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static GetMiscTypeMasterAutocompleteDto ValidAutoCompleteDto(
            int id = 1,
            string? miscTypeCode = "MTY001") =>
            new GetMiscTypeMasterAutocompleteDto
            {
                Id = id,
                MiscTypeCode = miscTypeCode
            };

        public static BudgetManagement.Domain.Entities.MiscTypeMaster ValidEntity(int id = 1) =>
            new BudgetManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = "MTY001",
                Description = "Test MiscType",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
