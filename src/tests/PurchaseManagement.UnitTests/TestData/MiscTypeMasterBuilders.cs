using PurchaseManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using PurchaseManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class MiscTypeMasterBuilders
    {
        public static CreateMiscTypeMasterCommand ValidCreateCommand(
            string code = "TYP001",
            string description = "Test Type") =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = code,
                Description = description
            };

        public static UpdateMiscTypeMasterCommand ValidUpdateCommand(
            int id = 1,
            string code = "TYP001",
            string description = "Updated Type",
            byte isActive = 1) =>
            new UpdateMiscTypeMasterCommand
            {
                Id = id,
                MiscTypeCode = code,
                Description = description,
                IsActive = isActive
            };

        public static DeleteMiscTypeMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteMiscTypeMasterCommand { Id = id };

        public static GetMiscTypeMasterDto ValidDto(int id = 1, string code = "TYP001") =>
            new GetMiscTypeMasterDto
            {
                Id = id,
                MiscTypeCode = code,
                Description = "Test Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static PurchaseManagement.Domain.Entities.MiscTypeMaster ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                Id = id,
                MiscTypeCode = "TYP001",
                Description = "Test Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
