using FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Command.UpdateAssetDisposal;
using FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetDisposalBuilders
    {
        public static CreateAssetDisposalCommand ValidCreateCommand(
            int assetId = 1,
            int assetPurchaseId = 1) =>
            new CreateAssetDisposalCommand
            {
                AssetId = assetId,
                AssetPurchaseId = assetPurchaseId,
                DisposalDate = new DateOnly(2025, 1, 15),
                DisposalType = 1,
                DisposalReason = "End of life",
                DisposalAmount = 5000m
            };

        public static UpdateAssetDisposalCommand ValidUpdateCommand(int id = 1) =>
            new UpdateAssetDisposalCommand
            {
                Id = id,
                DisposalDate = new DateOnly(2025, 1, 15),
                DisposalType = 1,
                DisposalReason = "Updated reason",
                DisposalAmount = 6000m
            };

        public static AssetDisposalDto ValidDto(int id = 1) =>
            new AssetDisposalDto
            {
                Id = id,
                AssetId = 1,
                AssetPurchaseId = 1,
                DisposalDate = new DateOnly(2025, 1, 15),
                DisposalType = 1,
                DisposalReason = "End of life",
                DisposalAmount = 5000m
            };

        public static FAM.Domain.Entities.AssetMaster.AssetDisposal ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetMaster.AssetDisposal
            {
                Id = id,
                AssetId = 1,
                AssetPurchaseId = 1,
                DisposalDate = new DateOnly(2025, 1, 15),
                DisposalType = 1,
                DisposalReason = "End of life",
                DisposalAmount = 5000m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
