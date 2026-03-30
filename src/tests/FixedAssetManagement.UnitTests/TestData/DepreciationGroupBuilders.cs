using FAM.Application.DepreciationGroup.Commands.CreateDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.UpdateDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class DepreciationGroupBuilders
    {
        public static CreateDepreciationGroupCommand ValidCreateCommand(
            string code = "DG001",
            string name = "TestDepGroup",
            int assetGroupId = 1,
            int depreciationMethod = 1,
            int bookType = 1) =>
            new CreateDepreciationGroupCommand
            {
                Code = code,
                DepreciationGroupName = name,
                AssetGroupId = assetGroupId,
                DepreciationMethod = depreciationMethod,
                BookType = bookType,
                UsefulLife = 5,
                ResidualValue = 10
            };

        public static UpdateDepreciationGroupCommand ValidUpdateCommand(
            int id = 1,
            string code = "DG001",
            string name = "UpdatedDepGroup",
            int assetGroupId = 1,
            int depreciationMethod = 1,
            int bookType = 1) =>
            new UpdateDepreciationGroupCommand
            {
                Id = id,
                Code = code,
                DepreciationGroupName = name,
                AssetGroupId = assetGroupId,
                DepreciationMethod = depreciationMethod,
                BookType = bookType,
                UsefulLife = 5,
                ResidualValue = 10,
                SortOrder = 1,
                IsActive = Status.Active
            };

        public static DeleteDepreciationGroupCommand ValidDeleteCommand(int id = 1) =>
            new DeleteDepreciationGroupCommand { Id = id };

        public static DepreciationGroupDTO ValidDto(int id = 1) =>
            new DepreciationGroupDTO
            {
                Id = id,
                Code = "DG001",
                DepreciationGroupName = "TestDepGroup",
                AssetGroupId = 1,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static DepreciationGroupAutoCompleteDTO ValidAutoCompleteDto(int id = 1) =>
            new DepreciationGroupAutoCompleteDTO
            {
                Id = id,
                Code = "DG001",
                DepreciationGroupName = "TestDepGroup"
            };

        public static FAM.Domain.Entities.DepreciationGroups ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.DepreciationGroups
            {
                Id = id,
                Code = "DG001",
                DepreciationGroupName = "TestDepGroup",
                AssetGroupId = 1,
                BookType = 1,
                DepreciationMethod = 1,
                UsefulLife = 5,
                ResidualValue = 10,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
