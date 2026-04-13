using FAM.Application.ExcelImport;
using OfficeOpenXml;

namespace FixedAssetManagement.UnitTests.Application.ExcelImport
{
    public sealed class AssetInsuranceHandlerTests
    {
        static AssetInsuranceHandlerTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private static ExcelWorksheet CreateWorksheet(
            string? policyNo = null,
            decimal? amount = null,
            string? vendorCode = null)
        {
            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            if (policyNo != null) ws.Cells[3, 48].Value = policyNo;
            if (amount.HasValue) ws.Cells[3, 51].Value = amount.Value;
            if (vendorCode != null) ws.Cells[3, 52].Value = vendorCode;
            return ws;
        }

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var ws = CreateWorksheet();
            var sut = new AssetInsuranceHandler(ws, 3);
            sut.Should().NotBeNull();
        }

        [Fact]
        public void ProcessAssetInsurance_WhenNoPolicy_ReturnsNull()
        {
            var ws = CreateWorksheet();
            var sut = new AssetInsuranceHandler(ws, 3);

            var result = sut.ProcessAssetInsurance();

            result.Should().BeNull();
        }

        [Fact]
        public void ProcessAssetInsurance_WhenValidPolicy_ReturnsSingleItem()
        {
            var ws = CreateWorksheet(policyNo: "POL001", amount: 2500m, vendorCode: "V001");
            var sut = new AssetInsuranceHandler(ws, 3);

            var result = sut.ProcessAssetInsurance();

            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].PolicyNo.Should().Be("POL001");
            result[0].VendorCode.Should().Be("V001");
        }

        [Fact]
        public void ProcessAssetInsurance_WhenAmountIsZero_ReturnsNull()
        {
            var ws = CreateWorksheet(policyNo: "POL001", amount: 0m);
            var sut = new AssetInsuranceHandler(ws, 3);

            var result = sut.ProcessAssetInsurance();

            result.Should().BeNull();
        }
    }
}
