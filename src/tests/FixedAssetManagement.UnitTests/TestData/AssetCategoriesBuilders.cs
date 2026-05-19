using FAM.Application.AssetCategories.Command.CreateAssetCategories;
using FAM.Application.AssetCategories.Command.DeleteAssetCategories;
using FAM.Application.AssetCategories.Command.UpdateAssetCategories;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetCategoriesBuilders
    {
        public static CreateAssetCategoriesCommand ValidCreateCommand(
            string categoryName = "TestCategory",
            int assetGroupId = 1,
            string description = "Testdescription") =>
            new CreateAssetCategoriesCommand
            {
                CategoryName = categoryName,
                Description = description,
                AssetGroupId = assetGroupId
            };

        public static UpdateAssetCategoriesCommand ValidUpdateCommand(
            int id = 1,
            string categoryName = "UpdatedCategory",
            int assetGroupId = 1,
            string description = "Updateddescription") =>
            new UpdateAssetCategoriesCommand
            {
                Id = id,
                CategoryName = categoryName,
                Description = description,
                AssetGroupId = assetGroupId,
                IsActive = 1
            };

        public static DeleteAssetCategoriesCommand ValidDeleteCommand(int id = 1) =>
            new DeleteAssetCategoriesCommand { Id = id };

        public static AssetCategoriesDto ValidDto(int id = 1) =>
            new AssetCategoriesDto
            {
                Id = id,
                Code = "TESTC",
                CategoryName = "TestCategory",
                Description = "Testdescription",
                SortOrder = 1,
                AssetGroupId = 1,
                AssetGroupName = "TestGroup",
                IsActive = Status.Active
            };

        public static AssetCategoriesAutoCompleteDto ValidAutoCompleteDto(int id = 1) =>
            new AssetCategoriesAutoCompleteDto
            {
                Id = id,
                CategoryName = "TestCategory"
            };

        public static FAM.Domain.Entities.AssetCategories ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetCategories
            {
                Id = id,
                Code = "TESTC",
                CategoryName = "TestCategory",
                Description = "Testdescription",
                SortOrder = 1,
                AssetGroupId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
