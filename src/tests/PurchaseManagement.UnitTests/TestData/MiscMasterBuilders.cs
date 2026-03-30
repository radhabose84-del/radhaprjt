using PurchaseManagement.Application.MiscMaster.Command.CreateMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using PurchaseManagement.Application.MiscMaster.Command.UpdateMiscMaster;
using PurchaseManagement.Application.MiscMaster.Queries.GetMiscMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class MiscMasterBuilders
    {
        public static CreateMiscMasterCommand ValidCreateCommand(
            string code = "MSC001",
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
            string code = "MSC001",
            string description = "Updated Misc",
            int miscTypeId = 1,
            byte isActive = 1) =>
            new UpdateMiscMasterCommand
            {
                Id = id,
                Code = code,
                Description = description,
                MiscTypeId = miscTypeId,
                IsActive = isActive
            };

        public static DeleteMiscMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscMasterCommand { Id = id };

        public static GetMiscMasterDto ValidDto(int id = 1, string code = "MSC001") =>
            new GetMiscMasterDto
            {
                Id = id,
                Code = code,
                Description = "Test Misc",
                MiscTypeId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static PurchaseManagement.Domain.Entities.MiscMaster ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.MiscMaster
            {
                Id = id,
                Code = "MSC001",
                Description = "Test Misc",
                MiscTypeId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
