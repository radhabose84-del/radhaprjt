using FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetSubCategoriesBuilders
    {
        public static CreateAssetSubCategoriesCommand ValidCreateCommand(
            string subCategoryName = "TestSubCategory",
            int assetCategoriesId = 1) =>
            new CreateAssetSubCategoriesCommand
            {
                SubCategoryName = subCategoryName,
                Description = "Testdescription",
                AssetCategoriesId = assetCategoriesId
            };

        public static UpdateAssetSubCategoriesCommand ValidUpdateCommand(
            int id = 1,
            string subCategoryName = "UpdatedSubCategory",
            int assetCategoriesId = 1) =>
            new UpdateAssetSubCategoriesCommand
            {
                Id = id,
                SubCategoryName = subCategoryName,
                Description = "Updateddescription",
                SortOrder = 1,
                AssetCategoriesId = assetCategoriesId,
                IsActive = 1
            };

        public static DeleteAssetSubCategoriesCommand ValidDeleteCommand(int id = 1) =>
            new DeleteAssetSubCategoriesCommand { Id = id };

        public static AssetSubCategoriesDto ValidDto(int id = 1) =>
            new AssetSubCategoriesDto
            {
                Id = id,
                Code = "TESTS",
                SubCategoryName = "TestSubCategory",
                Description = "Testdescription",
                SortOrder = 1,
                AssetCategoriesId = 1,
                AssetCategoriesName = "TestCategory",
                IsActive = Status.Active
            };

        public static AssetSubCategoriesAutoCompleteDto ValidAutoCompleteDto(int id = 1) =>
            new AssetSubCategoriesAutoCompleteDto
            {
                Id = id,
                SubCategoryName = "TestSubCategory"
            };

        public static FAM.Domain.Entities.AssetSubCategories ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetSubCategories
            {
                Id = id,
                Code = "TESTS",
                SubCategoryName = "TestSubCategory",
                Description = "Testdescription",
                SortOrder = 1,
                AssetCategoriesId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
