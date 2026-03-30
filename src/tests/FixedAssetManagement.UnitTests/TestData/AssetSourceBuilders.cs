using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetSourceAutoComplete;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetSourceBuilders
    {
        public static AssetSourceAutoCompleteDto ValidAutoCompleteDto(int id = 1) =>
            new AssetSourceAutoCompleteDto
            {
                Id = id,
                SourceName = "Test Source"
            };

        public static FAM.Domain.Entities.AssetSource ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetSource
            {
                Id = id,
                SourceCode = "SRC001",
                SourceName = "Test Source",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
