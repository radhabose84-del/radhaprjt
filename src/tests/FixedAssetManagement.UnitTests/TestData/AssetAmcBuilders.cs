using FAM.Application.AssetMaster.AssetAmc.Command.CreateAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Command.DeleteAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Command.UpdateAssetAmc;
using FAM.Application.AssetMaster.AssetAmc.Queries.GetAssetAmc;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetAmcBuilders
    {
        public static CreateAssetAmcCommand ValidCreateCommand(
            int assetId = 1,
            string vendorCode = "VEND001",
            string vendorName = "TestVendor") =>
            new CreateAssetAmcCommand
            {
                AssetId = assetId,
                StartDate = new DateOnly(2025, 1, 1),
                Period = 12,
                VendorCode = vendorCode,
                VendorName = vendorName,
                VendorPhone = "9876543210",
                VendorEmail = "test@vendor.com",
                CoverageType = 1,
                FreeServiceCount = 2,
                RenewalStatus = 1,
                RenewedDate = new DateOnly(2025, 12, 31),
                IsActive = 1
            };

        public static UpdateAssetAmcCommand ValidUpdateCommand(
            int id = 1,
            int assetId = 1,
            string vendorCode = "VEND001",
            string vendorName = "UpdatedVendor") =>
            new UpdateAssetAmcCommand
            {
                Id = id,
                AssetId = assetId,
                StartDate = new DateOnly(2025, 1, 1),
                Period = 12,
                VendorCode = vendorCode,
                VendorName = vendorName,
                VendorPhone = "9876543210",
                VendorEmail = "test@vendor.com",
                CoverageType = 1,
                FreeServiceCount = 2,
                RenewalStatus = 1,
                RenewedDate = new DateOnly(2025, 12, 31),
                IsActive = 1
            };

        public static DeleteAssetAmcCommand ValidDeleteCommand(int id = 1) =>
            new DeleteAssetAmcCommand { Id = id };

        public static AssetAmcDto ValidDto(int id = 1) =>
            new AssetAmcDto
            {
                Id = id,
                AssetId = 1,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 12, 31),
                Period = 12,
                VendorCode = "VEND001",
                VendorName = "TestVendor",
                CoverageType = 1,
                RenewalStatus = 1
            };

        public static FAM.Domain.Entities.AssetMaster.AssetAmc ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetMaster.AssetAmc
            {
                Id = id,
                AssetId = 1,
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 12, 31),
                Period = 12,
                VendorCode = "VEND001",
                VendorName = "TestVendor",
                CoverageType = 1,
                RenewalStatus = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
