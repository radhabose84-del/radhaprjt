using PartyManagement.Application.MiscMaster.Command.CreateMiscMaster;
using PartyManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PartyManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PartyManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.UnitTests.TestData
{
    public static class MiscMasterBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            string code = "MC001",
            string description = "Test Misc Master",
            int miscTypeId = 1) =>
            new CreateMiscMasterCommand
            {
                Code = code,
                Description = description,
                MiscTypeId = miscTypeId
            };

        public static UpdateMiscMasterCommand ValidUpdateCommand(
            int id = 1,
            string code = "MC001",
            string description = "Updated Misc Master",
            int miscTypeId = 1) =>
            new UpdateMiscMasterCommand
            {
                Id = id,
                Code = code,
                Description = description,
                MiscTypeId = miscTypeId,
                IsActive = 1,
                SortOrder = 1
            };

        public static DeleteMiscMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscMasterCommand { Id = id };

        public static GetMiscMasterDto ValidDto(int id = 1) =>
            new GetMiscMasterDto
            {
                Id = id,
                Code = "MC001",
                Description = "Test Misc Master",
                MiscTypeId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static PartyManagement.Domain.Entities.MiscMaster ValidEntity(int id = 1) =>
            new PartyManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                Code = "MC001",
                Description = "Test Misc Master",
                MiscTypeId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
