using FAM.Application.AssetMaster.AssetPurchase.Commands.CreateAssetPurchaseDetails;
using FAM.Application.AssetMaster.AssetPurchase.Queries.GetAssetPurchase;
using FAM.Domain.Entities.AssetPurchase;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetPurchaseBuilders
    {
        public static CreateAssetPurchaseDetailCommand ValidCreateCommand(int assetId = 1) =>
            new CreateAssetPurchaseDetailCommand
            {
                AssetId = assetId,
                AssetSourceId = 1,
                ItemName = "Test Item",
                ItemCode = "ITM001",
                GrnNo = 1001,
                GrnSno = 1,
                PoNo = 2001,
                PoSno = 1,
                PurchaseValue = 50000m,
                GrnValue = 50000m,
                AcceptedQty = 1,
                GrnDate = DateTimeOffset.UtcNow,
                PoDate = DateTimeOffset.UtcNow,
                BillDate = DateTimeOffset.UtcNow,
                PjDocNo = 3001
            };

        public static AssetPurchaseDetailsDto ValidDto(int id = 1) =>
            new AssetPurchaseDetailsDto
            {
                Id = id,
                AssetId = 1,
                AssetSourceId = 1,
                ItemName = "Test Item",
                ItemCode = "ITM001",
                PurchaseValue = 50000m
            };

        public static AssetPurchaseDetails ValidEntity(int id = 1) =>
            new AssetPurchaseDetails
            {
                Id = id,
                AssetId = 1,
                AssetSourceId = 1,
                ItemName = "Test Item",
                PurchaseValue = 50000m
            };
    }
}
