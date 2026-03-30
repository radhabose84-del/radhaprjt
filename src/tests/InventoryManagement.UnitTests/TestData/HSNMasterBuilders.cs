using InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.DeleteHSNMaster;
using InventoryManagement.Application.HSNMaster.Command.UpdateHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetAllHSNMaster;
using InventoryManagement.Application.HSNMaster.Queries.GetHSNMasterAutoComplete;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.TestData
{
    public static class HSNMasterBuilders
    {
        public static CreateHSNMasterCommand ValidCreateCommand(
            string hsnCode = "1001",
            int typeId = 1,
            int gstCategoryId = 2,
            decimal gstPercentage = 18m) =>
            new CreateHSNMasterCommand
            {
                HSNCode = hsnCode,
                Description = "Test HSN Description",
                TypeId = typeId,
                GSTCategoryId = gstCategoryId,
                GSTPercentage = gstPercentage,
                IGSTPercentage = 18m,
                ValidFrom = DateTimeOffset.UtcNow
            };

        public static UpdateHSNMasterCommand ValidUpdateCommand(
            int id = 1,
            string hsnCode = "1001") =>
            new UpdateHSNMasterCommand
            {
                Id = id,
                HSNCode = hsnCode,
                Description = "Updated Description",
                TypeId = 1,
                GSTCategoryId = 2,
                GSTPercentage = 18m,
                IGSTPercentage = 18m,
                ValidFrom = DateTimeOffset.UtcNow,
                IsActive = 1
            };

        public static DeleteHSNMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteHSNMasterCommand { Id = id };

        public static HSNMasterDto ValidDto(int id = 1, string hsnCode = "1001") =>
            new HSNMasterDto
            {
                Id = id,
                HSNCode = hsnCode,
                Description = "Test HSN Description",
                TypeId = 1,
                GSTPercentage = 18m,
                CGSTPercentage = 9m,
                SGSTPercentage = 9m,
                IGSTPercentage = 18m,
                ValidFrom = DateTimeOffset.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

        public static List<GetHSNMasterAutoCompleteDto> ValidAutoCompleteList() =>
            new List<GetHSNMasterAutoCompleteDto>
            {
                new GetHSNMasterAutoCompleteDto { Id = 1, HSNCode = "1001", HSNDescription = "Test", TypeCode = "GOODS" }
            };

        public static InventoryManagement.Domain.Entities.HSNMaster ValidEntity(int id = 1) =>
            new InventoryManagement.Domain.Entities.HSNMaster
            {
                Id = id,
                HSNCode = "1001",
                Description = "Test HSN Description",
                TypeId = 1,
                GSTCategoryId = 2,
                GSTPercentage = 18m,
                IGSTPercentage = 18m,
                ValidFrom = DateTimeOffset.UtcNow,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
