using FAM.Application.AssetGroup.Command.CreateAssetGroup;
using FAM.Application.AssetGroup.Command.DeleteAssetGroup;
using FAM.Application.AssetGroup.Command.UpdateAssetGroup;
using FAM.Application.AssetGroup.Queries.GetAssetGroup;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetGroupBuilders
    {
        public static CreateAssetGroupCommand ValidCreateCommand(
            string code = "AG001",
            string groupName = "Test Asset Group") =>
            new CreateAssetGroupCommand
            {
                Code = code,
                GroupName = groupName,
                GroupPercentage = 10.0m
            };

        public static UpdateAssetGroupCommand ValidUpdateCommand(
            int id = 1,
            string groupName = "Updated Asset Group") =>
            new UpdateAssetGroupCommand
            {
                Id = id,
                GroupName = groupName,
                SortOrder = 1,
                IsActive = 1,
                GroupPercentage = 10.0m
            };

        public static DeleteAssetGroupCommand ValidDeleteCommand(int id = 1) =>
            new DeleteAssetGroupCommand { Id = id };

        public static AssetGroupDto ValidDto(int id = 1) =>
            new AssetGroupDto
            {
                Id = id,
                Code = "AG001",
                GroupName = "Test Asset Group",
                SortOrder = 1,
                IsActive = Status.Active,
                GroupPercentage = 10.0m
            };

        public static AssetGroupAutoCompleteDTO ValidAutoCompleteDto(int id = 1) =>
            new AssetGroupAutoCompleteDTO
            {
                Id = id,
                GroupName = "Test Asset Group"
            };

        public static FAM.Domain.Entities.AssetGroup ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetGroup
            {
                Id = id,
                Code = "AG001",
                GroupName = "Test Asset Group",
                SortOrder = 1,
                GroupPercentage = 10.0m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
