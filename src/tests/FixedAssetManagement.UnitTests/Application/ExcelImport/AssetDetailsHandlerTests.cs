using Contracts.Interfaces;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.ExcelImport;
using OfficeOpenXml;

namespace FixedAssetManagement.UnitTests.Application.ExcelImport
{
    public sealed class AssetDetailsHandlerTests
    {
        static AssetDetailsHandlerTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private readonly Mock<IExcelImportCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IExcelImportQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private AssetDetailsHandler CreateSut()
        {
            _mockIp.Setup(i => i.GetCompanyId()).Returns(1);
            _mockIp.Setup(i => i.GetUnitId()).Returns(2);
            return new AssetDetailsHandler(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object);
        }

        private static ExcelWorksheet CreateWorksheet()
        {
            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            ws.Cells[3, 10].Value = "ParentAsset";
            ws.Cells[3, 12].Value = "IT";
            ws.Cells[3, 13].Value = "Loc1";
            ws.Cells[3, 14].Value = "SubLoc1";
            ws.Cells[3, 15].Value = "42";
            return ws;
        }

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task ProcessAssetDetailsAsync_SetsCompanyAndUnitIds()
        {
            _mockCommandRepo.Setup(r => r.GetAssetIdByNameAsync(It.IsAny<string>())).ReturnsAsync(99);
            _mockCommandRepo.Setup(r => r.GetAssetLocationIdByNameAsync(It.IsAny<string>())).ReturnsAsync(10);
            _mockCommandRepo.Setup(r => r.GetAssetSubLocationIdByNameAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(20);
            _mockQueryRepo.Setup(r => r.GetAssetDeptIdByNameAsync(It.IsAny<string>())).ReturnsAsync(5);

            var ws = CreateWorksheet();
            var dto = new AssetMasterDto();
            var request = new ImportAssetCommand(new ImportAssetDto());

            await CreateSut().ProcessAssetDetailsAsync(request, ws, 3, dto);

            dto.CompanyId.Should().Be(1);
            dto.UnitId.Should().Be(2);
            dto.AssetParentId.Should().Be(99);
        }

        [Fact]
        public async Task ProcessAssetDetailsAsync_WhenAssetParentEmpty_SetsNull()
        {
            _mockCommandRepo.Setup(r => r.GetAssetLocationIdByNameAsync(It.IsAny<string>())).ReturnsAsync(10);
            _mockCommandRepo.Setup(r => r.GetAssetSubLocationIdByNameAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(20);
            _mockQueryRepo.Setup(r => r.GetAssetDeptIdByNameAsync(It.IsAny<string>())).ReturnsAsync(5);

            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            // Row 3: col 10 (parent) is empty
            ws.Cells[3, 12].Value = "IT";

            var dto = new AssetMasterDto();
            var request = new ImportAssetCommand(new ImportAssetDto());

            await CreateSut().ProcessAssetDetailsAsync(request, ws, 3, dto);

            dto.AssetParentId.Should().BeNull();
        }

        [Fact]
        public async Task ProcessAssetDetailsAsync_PopulatesAssetLocation()
        {
            _mockCommandRepo.Setup(r => r.GetAssetIdByNameAsync(It.IsAny<string>())).ReturnsAsync(99);
            _mockCommandRepo.Setup(r => r.GetAssetLocationIdByNameAsync(It.IsAny<string>())).ReturnsAsync(10);
            _mockCommandRepo.Setup(r => r.GetAssetSubLocationIdByNameAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(20);
            _mockQueryRepo.Setup(r => r.GetAssetDeptIdByNameAsync(It.IsAny<string>())).ReturnsAsync(5);

            var ws = CreateWorksheet();
            var dto = new AssetMasterDto();
            var request = new ImportAssetCommand(new ImportAssetDto());

            await CreateSut().ProcessAssetDetailsAsync(request, ws, 3, dto);

            dto.AssetLocation.Should().NotBeNull();
            dto.AssetLocation!.LocationId.Should().Be(10);
            dto.AssetLocation.SubLocationId.Should().Be(20);
            dto.AssetLocation.DepartmentId.Should().Be(5);
            dto.AssetLocation.CustodianId.Should().Be(42);
        }
    }
}
