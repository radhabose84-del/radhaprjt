using FAM.Application.DepreciationDetail.Commands.CreateDepreciationDetail;
using FAM.Application.DepreciationDetail.Commands.DeleteDepreciationDetail;
using FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class DepreciationDetailBuilders
    {
        public static CreateDepreciationDetailCommand ValidCreateCommand(
            int finYearId = 1,
            int depreciationType = 1,
            int depreciationPeriod = 1) =>
            new CreateDepreciationDetailCommand
            {
                companyId = 1,
                unitId = 1,
                finYearId = finYearId,
                depreciationType = depreciationType,
                depreciationPeriod = depreciationPeriod
            };

        public static DeleteDepreciationDetailCommand ValidDeleteCommand(
            int finYearId = 1,
            int depreciationType = 1,
            int depreciationPeriod = 1) =>
            new DeleteDepreciationDetailCommand
            {
                companyId = 1,
                unitId = 1,
                finYearId = finYearId,
                depreciationType = depreciationType,
                depreciationPeriod = depreciationPeriod
            };

        public static DepreciationDto ValidDto() =>
            new DepreciationDto
            {
                AssetId = 1,
                Company = "Test Company",
                Unit = "Test Unit",
                AssetCode = "AST001",
                AssetName = "Test Asset",
                PurchaseCost = 100000m,
                OpeningValue = 90000m,
                DepreciationValue = 10000m,
                ClosingValue = 80000m
            };
    }
}
