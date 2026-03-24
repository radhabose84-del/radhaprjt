using InventoryManagement.Application.MiscMaster.Command.CreateMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using InventoryManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using InventoryManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.TestData
{
    public static class MiscMasterBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            string code = "MISC001",
            string description = "Test Misc",
            int miscTypeId = 1) =>
            new CreateMiscMasterCommand
            {
                Code = code,
                Description = description,
                MiscTypeId = miscTypeId
            };

        public static UpdateMiscMasterCommand ValidUpdateCommand(
            int id = 1,
            string code = "MISC001",
            string description = "Updated Misc",
            int miscTypeId = 1) =>
            new UpdateMiscMasterCommand
            {
                Id = id,
                Code = code,
                Description = description,
                MiscTypeId = miscTypeId,
                SortOrder = 1,
                IsActive = 1
            };

        public static DeleteMiscMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscMasterCommand { Id = id };

        public static GetMiscMasterDto ValidDto(int id = 1) =>
            new GetMiscMasterDto
            {
                Id = id,
                Code = "MISC001",
                Description = "Test Misc",
                MiscTypeId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static InventoryManagement.Domain.Entities.MiscMaster ValidEntity(int id = 1) =>
            new InventoryManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                Code = "MISC001",
                Description = "Test Misc",
                MiscTypeId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
