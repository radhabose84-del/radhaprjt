using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost;
using FAM.Application.AssetMaster.AssetAdditionalCost.Commands.UpdateAssetAdditionalCost;
using FAM.Application.AssetMaster.AssetAdditionalCost.Queries.GetAssetAdditionalCost;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetAdditionalCostBuilders
    {
        public static CreateAssetAdditionalCostCommand ValidCreateCommand(
            int assetId = 1,
            int assetSourceId = 1,
            decimal amount = 5000m) =>
            new CreateAssetAdditionalCostCommand
            {
                AssetId = assetId,
                AssetSourceId = assetSourceId,
                Amount = amount,
                JournalNo = "JNL001",
                CostType = 1
            };

        public static UpdateAssetAdditionalCostCommand ValidUpdateCommand(
            int id = 1,
            decimal amount = 6000m) =>
            new UpdateAssetAdditionalCostCommand
            {
                Id = id,
                Amount = amount,
                JournalNo = "JNL002",
                CostType = 2
            };

        public static AssetAdditionalCostDto ValidDto(int id = 1) =>
            new AssetAdditionalCostDto
            {
                Id = id,
                AssetId = 1,
                AssetSourceId = 1,
                Amount = 5000m,
                JournalNo = "JNL001",
                CostType = 1
            };

        public static FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetPurchase.AssetAdditionalCost
            {
                Id = id,
                AssetId = 1,
                AssetSourceId = 1,
                Amount = 5000m,
                JournalNo = "JNL001",
                CostType = 1
            };
    }
}
