using FAM.Application.ExcelImport;
using OfficeOpenXml;

namespace FixedAssetManagement.UnitTests.Application.ExcelImport
{
    public sealed class AssetAdditionalCostHandlerTests
    {
        static AssetAdditionalCostHandlerTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private static ExcelWorksheet CreateWorksheetWithAmount(decimal? amount)
        {
            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            // Seed enough cells so Dimension is non-null
            ws.Cells[1, 1].Value = "header";
            if (amount.HasValue)
            {
                ws.Cells[3, 47].Value = amount.Value;
            }
            return ws;
        }

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var ws = CreateWorksheetWithAmount(null);
            var sut = new AssetAdditionalCostHandler(ws, 3);
            sut.Should().NotBeNull();
        }

        [Fact]
        public void ProcessAssetAdditionalCost_WhenAmountIsZero_ReturnsNull()
        {
            var ws = CreateWorksheetWithAmount(null);
            var sut = new AssetAdditionalCostHandler(ws, 3);

            var result = sut.ProcessAssetAdditionalCost();

            result.Should().BeNull();
        }

        [Fact]
        public void ProcessAssetAdditionalCost_WhenAmountIsPositive_ReturnsListWithOneItem()
        {
            var ws = CreateWorksheetWithAmount(1500m);
            var sut = new AssetAdditionalCostHandler(ws, 3);

            var result = sut.ProcessAssetAdditionalCost();

            result.Should().NotBeNull();
            result!.Should().HaveCount(1);
            result![0].Amount.Should().Be(1500m);
            result[0].CostType.Should().Be(57);
            result[0].AssetSourceId.Should().Be(2);
        }

        [Fact]
        public void ProcessAssetAdditionalCost_WhenAmountIsNegative_ReturnsNull()
        {
            var ws = CreateWorksheetWithAmount(-10m);
            var sut = new AssetAdditionalCostHandler(ws, 3);

            var result = sut.ProcessAssetAdditionalCost();

            result.Should().BeNull();
        }
    }
}
