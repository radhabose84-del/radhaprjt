using Contracts.Interfaces;
using FAM.Application.ExcelImport;
using OfficeOpenXml;

namespace FixedAssetManagement.UnitTests.Application.ExcelImport
{
    public sealed class AssetPurchaseHandlerTests
    {
        static AssetPurchaseHandlerTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private AssetPurchaseHandler CreateSut(ExcelWorksheet ws, int row = 3)
        {
            _mockIp.Setup(i => i.GetOldUnitId()).Returns("OLD1");
            return new AssetPurchaseHandler(ws, row, _mockIp.Object);
        }

        private static ExcelWorksheet CreateEmptyWorksheet()
        {
            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            return ws;
        }

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var ws = CreateEmptyWorksheet();
            var sut = CreateSut(ws);
            sut.Should().NotBeNull();
        }

        [Fact]
        public void ProcessAssetPurchase_WhenAllCellsEmpty_ReturnsNull()
        {
            var ws = CreateEmptyWorksheet();
            var sut = CreateSut(ws);

            var result = sut.ProcessAssetPurchase();

            result.Should().BeNull();
        }

        [Fact]
        public void ProcessAssetPurchase_WhenVendorCodePresent_ReturnsSingleItem()
        {
            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            ws.Cells[3, 36].Value = "VC001";  // VendorCode
            ws.Cells[3, 37].Value = "Acme";   // VendorName
            ws.Cells[3, 43].Value = 5000m;    // PurchaseValue

            var sut = CreateSut(ws);
            var result = sut.ProcessAssetPurchase();

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].VendorCode.Should().Be("VC001");
            result[0].VendorName.Should().Be("Acme");
            result[0].PurchaseValue.Should().Be(5000m);
            result[0].OldUnitId.Should().Be("OLD1");
        }
    }
}
