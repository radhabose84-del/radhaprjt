using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetMasterGeneralBuilders
    {
        public static CreateAssetMasterGeneralCommand ValidCreateCommand() =>
            new CreateAssetMasterGeneralCommand
            {
                AssetMaster = new AssetMasterDto
                {
                    AssetName = "Test Asset",
                    CompanyId = 1,
                    UnitId = 1,
                    AssetGroupId = 1,
                    AssetCategoryId = 1,
                    AssetSubCategoryId = 1
                }
            };

        public static DeleteAssetMasterGeneralCommand ValidDeleteCommand(int id = 1) =>
            new DeleteAssetMasterGeneralCommand { Id = id };

        public static AssetMasterGeneralDTO ValidDto(int id = 1) =>
            new AssetMasterGeneralDTO
            {
                Id = id,
                AssetCode = "AST001",
                AssetName = "Test Asset",
                CompanyId = 1,
                UnitId = 1,
                AssetGroupId = 1,
                AssetCategoryId = 1,
                AssetSubCategoryId = 1,
                IsActive = Status.Active
            };

        public static AssetMasterDto ValidAssetMasterDto(int id = 1) =>
            new AssetMasterDto
            {
                Id = id,
                AssetName = "Test Asset",
                CompanyId = 1,
                UnitId = 1,
                AssetGroupId = 1,
                AssetCategoryId = 1,
                AssetSubCategoryId = 1
            };

        public static AssetMasterGenerals ValidEntity(int id = 1) =>
            new AssetMasterGenerals
            {
                Id = id,
                AssetCode = "AST001",
                AssetName = "Test Asset",
                CompanyId = 1,
                UnitId = 1,
                AssetGroupId = 1,
                AssetCategoryId = 1,
                AssetSubCategoryId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
