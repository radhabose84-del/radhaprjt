using FAM.Application.AssetMaster.AssetWarranty.Commands.CreateAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetWarrantyBuilders
    {
        public static CreateAssetWarrantyCommand ValidCreateCommand(int assetId = 1) =>
            new CreateAssetWarrantyCommand
            {
                AssetId = assetId,
                WarrantyType = 1,
                Description = "Standard Warranty"
            };

        public static DeleteAssetWarrantyCommand ValidDeleteCommand(int id = 1) =>
            new DeleteAssetWarrantyCommand { Id = id };

        public static AssetWarrantyDTO ValidDto(int id = 1) =>
            new AssetWarrantyDTO
            {
                Id = id,
                AssetId = 1,
                WarrantyType = 1,
                Description = "Standard Warranty",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        public static AssetWarranties ValidEntity(int id = 1) =>
            new AssetWarranties
            {
                Id = id,
                AssetId = 1,
                WarrantyType = 1,
                Description = "Standard Warranty",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
