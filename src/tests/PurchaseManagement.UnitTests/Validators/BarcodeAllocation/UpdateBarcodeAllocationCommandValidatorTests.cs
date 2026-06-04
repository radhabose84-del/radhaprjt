using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Presentation.Validation.BarcodeAllocation;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.BarcodeAllocation
{
    public sealed class UpdateBarcodeAllocationCommandValidatorTests
    {
        private readonly Mock<IBarcodeAllocationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateBarcodeAllocationCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(bool exists = true, bool seriesExists = true, bool overlaps = false, bool within = true, bool used = false)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(exists);
            _mockQueryRepo.Setup(r => r.SeriesExistsAsync(It.IsAny<int>())).ReturnsAsync(seriesExists);
            _mockQueryRepo.Setup(r => r.RangeOverlapsInSeriesAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int?>())).ReturnsAsync(overlaps);
            _mockQueryRepo.Setup(r => r.IsWithinSeriesRangeAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(within);
            _mockQueryRepo.Setup(r => r.IsUsedAsync(It.IsAny<int>())).ReturnsAsync(used);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(BarcodeAllocationBuilders.ValidUpdateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            SetupAllAsyncMocks(exists: false);
            var result = await CreateValidator().TestValidateAsync(BarcodeAllocationBuilders.ValidUpdateCommand(id: 99));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(BarcodeAllocationBuilders.ValidUpdateCommand(isActive: 5));
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_UsedAllocation_FailsValidation()
        {
            SetupAllAsyncMocks(used: true);
            var result = await CreateValidator().TestValidateAsync(BarcodeAllocationBuilders.ValidUpdateCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_RangeOutsideSeries_FailsValidation()
        {
            SetupAllAsyncMocks(within: false);
            var result = await CreateValidator().TestValidateAsync(BarcodeAllocationBuilders.ValidUpdateCommand());
            result.ShouldHaveValidationErrorFor("Barcode Range");
        }
    }
}
