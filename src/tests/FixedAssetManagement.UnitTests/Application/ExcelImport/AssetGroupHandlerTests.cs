using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.Common.Interfaces.IExcelImport;
using FAM.Application.ExcelImport;
using OfficeOpenXml;

namespace FixedAssetManagement.UnitTests.Application.ExcelImport
{
    public sealed class AssetGroupHandlerTests
    {
        static AssetGroupHandlerTests()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private readonly Mock<IExcelImportCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IExcelImportQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private AssetGroupHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object);

        private static ExcelWorksheet CreateFullyPopulatedWorksheet()
        {
            var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Sheet1");
            ws.Cells[1, 1].Value = "header";
            ws.Cells[3, 1].Value = "SubGrp";
            ws.Cells[3, 2].Value = "Grp";
            ws.Cells[3, 3].Value = "Cat";
            ws.Cells[3, 4].Value = "SubCat";
            ws.Cells[3, 5].Value = "Description";
            ws.Cells[3, 6].Value = "AssetName";
            ws.Cells[3, 7].Value = "5"; // Quantity
            ws.Cells[3, 8].Value = "Nos";
            ws.Cells[3, 9].Value = "true"; // Active
            return ws;
        }

        [Fact]
        public void Constructor_DoesNotThrow()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public async Task ProcessAssetGroupAsync_WhenAllLookupsSucceed_ReturnsDto()
        {
            _mockCommandRepo.Setup(r => r.GetAssetGroupIdByNameAsync(It.IsAny<string>())).ReturnsAsync(10);
            _mockCommandRepo.Setup(r => r.GetAssetSubGroupIdByNameAsync(It.IsAny<string>())).ReturnsAsync(20);
            _mockCommandRepo.Setup(r => r.GetAssetCategoryIdByNameAsync(It.IsAny<string>())).ReturnsAsync(30);
            _mockCommandRepo.Setup(r => r.GetAssetSubCategoryIdByNameAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(40);
            _mockCommandRepo.Setup(r => r.GetAssetUOMIdByNameAsync(It.IsAny<string>())).ReturnsAsync(50);

            var ws = CreateFullyPopulatedWorksheet();
            var request = new ImportAssetCommand(new ImportAssetDto());

            var result = await CreateSut().ProcessAssetGroupAsync(request, ws, 3);

            result.Should().NotBeNull();
            result.AssetGroupId.Should().Be(10);
            result.AssetSubGroupId.Should().Be(20);
            result.AssetCategoryId.Should().Be(30);
            result.AssetSubCategoryId.Should().Be(40);
            result.UOMId.Should().Be(50);
        }

        [Fact]
        public async Task ProcessAssetGroupAsync_WhenAssetGroupNotFound_ThrowsException()
        {
            _mockCommandRepo.Setup(r => r.GetAssetGroupIdByNameAsync(It.IsAny<string>())).ReturnsAsync((int?)null);

            var ws = CreateFullyPopulatedWorksheet();
            var request = new ImportAssetCommand(new ImportAssetDto());

            var sut = CreateSut();
            Func<Task> act = () => sut.ProcessAssetGroupAsync(request, ws, 3);

            await act.Should().ThrowAsync<Exception>().WithMessage("*Asset Group*");
        }

        [Fact]
        public async Task ProcessAssetGroupAsync_WhenSubGroupIsNull_StillReturnsDto()
        {
            _mockCommandRepo.Setup(r => r.GetAssetGroupIdByNameAsync(It.IsAny<string>())).ReturnsAsync(10);
            _mockCommandRepo.Setup(r => r.GetAssetSubGroupIdByNameAsync(It.IsAny<string>())).ReturnsAsync((int?)null);
            _mockCommandRepo.Setup(r => r.GetAssetCategoryIdByNameAsync(It.IsAny<string>())).ReturnsAsync(30);
            _mockCommandRepo.Setup(r => r.GetAssetSubCategoryIdByNameAsync(It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(40);
            _mockCommandRepo.Setup(r => r.GetAssetUOMIdByNameAsync(It.IsAny<string>())).ReturnsAsync(50);

            var ws = CreateFullyPopulatedWorksheet();
            var request = new ImportAssetCommand(new ImportAssetDto());

            var result = await CreateSut().ProcessAssetGroupAsync(request, ws, 3);

            result.AssetSubGroupId.Should().BeNull();
        }
    }
}
