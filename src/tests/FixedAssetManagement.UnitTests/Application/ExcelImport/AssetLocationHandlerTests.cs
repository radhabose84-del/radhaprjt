using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.ExcelImport;
using OfficeOpenXml;

namespace FixedAssetManagement.UnitTests.Application.ExcelImport
{
    public sealed class AssetLocationHandlerTests
    {
        static AssetLocationHandlerTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private readonly Mock<IExcelImportCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IExcelImportQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private AssetLocationHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object);

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task ProcessLocationAsync_WhenLocationEmpty_UsesDefaultLocation()
        {
            _mockQueryRepo.Setup(r => r.GetAssetDeptIdByNameAsync(It.IsAny<string>())).ReturnsAsync(5);

            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            ws.Cells[3, 12].Value = "IT";
            // col 13 (location) is empty

            var result = await CreateSut().ProcessLocationAsync(ws, 3);

            result.Should().NotBeNull();
            result.LocationId.Should().Be(1);
            result.SubLocationId.Should().Be(2);
            result.DepartmentId.Should().Be(5);
        }

        [Fact]
        public async Task ProcessLocationAsync_WhenDepartmentNotFound_ThrowsException()
        {
            _mockCommandRepo.Setup(r => r.GetAssetLocationIdByNameAsync(It.IsAny<string>())).ReturnsAsync(10);
            _mockCommandRepo.Setup(r => r.GetAssetSubLocationIdByNameAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(20);
            _mockQueryRepo.Setup(r => r.GetAssetDeptIdByNameAsync(It.IsAny<string>())).ReturnsAsync((int?)null);

            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            ws.Cells[3, 12].Value = "UnknownDept";
            ws.Cells[3, 13].Value = "Loc1";
            ws.Cells[3, 14].Value = "SubLoc1";

            var sut = CreateSut();
            Func<Task> act = () => sut.ProcessLocationAsync(ws, 3);

            await act.Should().ThrowAsync<Exception>().WithMessage("*Department*");
        }

        [Fact]
        public async Task ProcessLocationAsync_WhenLocationPopulated_ReturnsExpectedIds()
        {
            _mockCommandRepo.Setup(r => r.GetAssetLocationIdByNameAsync(It.IsAny<string>())).ReturnsAsync(10);
            _mockCommandRepo.Setup(r => r.GetAssetSubLocationIdByNameAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(20);
            _mockQueryRepo.Setup(r => r.GetAssetDeptIdByNameAsync(It.IsAny<string>())).ReturnsAsync(5);

            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            ws.Cells[3, 12].Value = "IT";
            ws.Cells[3, 13].Value = "Loc1";
            ws.Cells[3, 14].Value = "SubLoc1";
            ws.Cells[3, 15].Value = "42";

            var result = await CreateSut().ProcessLocationAsync(ws, 3);

            result.Should().NotBeNull();
            result.LocationId.Should().Be(10);
            result.SubLocationId.Should().Be(20);
            result.DepartmentId.Should().Be(5);
            result.CustodianId.Should().Be(42);
        }
    }
}
