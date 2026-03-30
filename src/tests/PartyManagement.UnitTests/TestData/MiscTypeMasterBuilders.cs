using PartyManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.UnitTests.TestData
{
    public static class MiscTypeMasterBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string code = "MTY001",
            string description = "Test Misc Type") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = code,
                Description = description
            };

        public static UpdateMiscTypeMasterCommand ValidUpdateCommand(
            int id = 1,
            string code = "MTY001",
            string description = "Updated Misc Type") =>
            new UpdateMiscTypeMasterCommand
            {
                Id = id,
                MiscTypeCode = code,
                Description = description,
                IsActive = 1
            };

        public static DeleteMiscTypeMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscTypeMasterCommand { Id = id };

        public static GetMiscTypeMasterDto ValidDto(int id = 1) =>
            new GetMiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = "MTY001",
                Description = "Test Misc Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static PartyManagement.Domain.Entities.MiscTypeMaster ValidEntity(int id = 1) =>
            new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = "MTY001",
                Description = "Test Misc Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
