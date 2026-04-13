using FAM.Application.ExcelImport;
using OfficeOpenXml;

namespace FixedAssetManagement.UnitTests.Application.ExcelImport
{
    public sealed class AssetSpecificationHandlerTests
    {
        static AssetSpecificationHandlerTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
            var sut = new AssetSpecificationHandler(ws, 3);
            sut.Should().NotBeNull();
        }

        [Fact]
        public void ProcessSpecifications_WhenAllCellsEmpty_ReturnsEmptyList()
        {
            var ws = CreateEmptyWorksheet();
            var sut = new AssetSpecificationHandler(ws, 3);

            var result = sut.ProcessSpecifications();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void ProcessSpecifications_WhenSomeCellsPopulated_ReturnsMatchingItems()
        {
            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            ws.Cells[3, 16].Value = "Samsung"; // Make
            ws.Cells[3, 17].Value = "XYZ-100"; // Model Number
            ws.Cells[3, 18].Value = "SN12345";  // Serial Number

            var sut = new AssetSpecificationHandler(ws, 3);

            var result = sut.ProcessSpecifications();

            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(s => s.SpecificationValue == "Samsung");
            result.Should().Contain(s => s.SpecificationValue == "XYZ-100");
            result.Should().Contain(s => s.SpecificationValue == "SN12345");
        }

        [Fact]
        public void ProcessSpecifications_WhenAllKnownColumnsFilled_Returns20Items()
        {
            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            for (int col = 16; col <= 35; col++)
            {
                ws.Cells[3, col].Value = $"val{col}";
            }

            var sut = new AssetSpecificationHandler(ws, 3);

            var result = sut.ProcessSpecifications();

            result.Should().HaveCount(20);
        }
    }
}
