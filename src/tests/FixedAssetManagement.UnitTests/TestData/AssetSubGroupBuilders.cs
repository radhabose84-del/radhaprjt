using FAM.Application.AssetSubGroup.Command.CreateAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.DeleteAssetSubGroup;
using FAM.Application.AssetSubGroup.Command.UpdateAssetSubGroup;
using FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetSubGroupBuilders
    {
        public static CreateAssetSubGroupCommand ValidCreateCommand(
            string code = "SG001",
            string subGroupName = "TestSubGroup",
            int groupId = 1) =>
            new CreateAssetSubGroupCommand
            {
                Code = code,
                SubGroupName = subGroupName,
                GroupId = groupId,
                SubGroupPercentage = 5.0m,
                AdditionalDepreciation = 0
            };

        public static UpdateAssetSubGroupCommand ValidUpdateCommand(
            int id = 1,
            string subGroupName = "UpdatedSubGroup",
            int groupId = 1) =>
            new UpdateAssetSubGroupCommand
            {
                Id = id,
                SubGroupName = subGroupName,
                SortOrder = 1,
                GroupId = groupId,
                IsActive = 1,
                SubGroupPercentage = 5.0m,
                AdditionalDepreciation = 0
            };

        public static DeleteAssetSubGroupCommand ValidDeleteCommand(int id = 1) =>
            new DeleteAssetSubGroupCommand { Id = id };

        public static AssetSubGroupDto ValidDto(int id = 1) =>
            new AssetSubGroupDto
            {
                Id = id,
                Code = "SG001",
                SubGroupName = "Test Sub Group",
                SortOrder = 1,
                GroupId = 1,
                IsActive = Status.Active,
                SubGroupPercentage = 5.0m,
                AdditionalDepreciation = 0
            };

        public static AssetSubGroupAutoCompleteDTO ValidAutoCompleteDto(int id = 1) =>
            new AssetSubGroupAutoCompleteDTO
            {
                Id = id,
                SubGroupName = "Test Sub Group"
            };

        public static FAM.Domain.Entities.AssetSubGroup ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetSubGroup
            {
                Id = id,
                Code = "SG001",
                SubGroupName = "Test Sub Group",
                SortOrder = 1,
                GroupId = 1,
                SubGroupPercentage = 5.0m,
                AdditionalDepreciation = 0,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
