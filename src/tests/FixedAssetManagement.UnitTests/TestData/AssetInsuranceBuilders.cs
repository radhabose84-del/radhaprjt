using FAM.Application.AssetMaster.AssetInsurance.Commands.CreateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.DeleteAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetInsuranceBuilders
    {
        public static CreateAssetInsuranceCommand ValidCreateCommand(
            int assetId = 1,
            string policyNo = "POL001") =>
            new CreateAssetInsuranceCommand
            {
                AssetId = assetId,
                PolicyNo = policyNo,
                StartDate = new DateOnly(2025, 1, 1),
                Insuranceperiod = 12,
                EndDate = new DateOnly(2025, 12, 31),
                PolicyAmount = 10000m,
                VendorCode = "VEND001",
                RenewalStatus = 1,
                RenewedDate = new DateOnly(2025, 12, 31),
                IsActive = 1
            };

        public static UpdateAssetInsuranceCommand ValidUpdateCommand(
            int id = 1,
            int assetId = 1,
            string policyNo = "POL001") =>
            new UpdateAssetInsuranceCommand
            {
                Id = id,
                AssetId = assetId,
                PolicyNo = policyNo,
                StartDate = new DateOnly(2025, 1, 1),
                Insuranceperiod = 12,
                EndDate = new DateOnly(2025, 12, 31),
                PolicyAmount = 10000m,
                VendorCode = "VEND001",
                RenewalStatus = 1,
                RenewedDate = new DateOnly(2025, 12, 31),
                IsActive = 1
            };

        public static DeleteAssetInsuranceCommand ValidDeleteCommand(int id = 1) =>
            new DeleteAssetInsuranceCommand { Id = id };

        public static GetAssetInsuranceDto ValidDto(int id = 1) =>
            new GetAssetInsuranceDto
            {
                Id = id,
                AssetId = 1,
                PolicyNo = "POL001",
                StartDate = new DateOnly(2025, 1, 1),
                EndDate = new DateOnly(2025, 12, 31),
                PolicyAmount = "10000",
                VendorCode = "VEND001",
                RenewalStatus = 1,
                RenewedDate = new DateOnly(2025, 12, 31),
                IsActive = Status.Active
            };

        public static FAM.Domain.Entities.AssetMaster.AssetInsurance ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.AssetMaster.AssetInsurance
            {
                Id = id,
                AssetId = 1,
                PolicyNo = "POL001",
                StartDate = new DateOnly(2025, 1, 1),
                Insuranceperiod = 12,
                EndDate = new DateOnly(2025, 12, 31),
                PolicyAmount = 10000m,
                VendorCode = "VEND001",
                RenewalStatus = 1,
                RenewedDate = new DateOnly(2025, 12, 31),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
