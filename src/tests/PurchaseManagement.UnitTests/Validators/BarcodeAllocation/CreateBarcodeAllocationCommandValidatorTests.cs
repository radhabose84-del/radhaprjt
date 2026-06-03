using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeAllocation;
using PurchaseManagement.Presentation.Validation.BarcodeAllocation;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.BarcodeAllocation
{
    public sealed class CreateBarcodeAllocationCommandValidatorTests
    {
        private readonly Mock<IBarcodeAllocationQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateBarcodeAllocationCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(bool seriesExists = true, bool overlaps = false, bool within = true)
        {
            _mockQueryRepo.Setup(r => r.SeriesExistsAsync(It.IsAny<int>())).ReturnsAsync(seriesExists);
            _mockQueryRepo.Setup(r => r.RangeOverlapsInSeriesAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int?>())).ReturnsAsync(overlaps);
            _mockQueryRepo.Setup(r => r.IsWithinSeriesRangeAsync(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>())).ReturnsAsync(within);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(BarcodeAllocationBuilders.ValidCreateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_MissingSeries_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = BarcodeAllocationBuilders.ValidCreateCommand(seriesId: 0);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.BarcodeSeriesId);
        }

        [Fact]
        public async Task Validate_InvalidSeries_FailsValidation()
        {
            SetupAllAsyncMocks(seriesExists: false);
            var command = BarcodeAllocationBuilders.ValidCreateCommand(seriesId: 999);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.BarcodeSeriesId);
        }

        [Fact]
        public async Task Validate_EmptyEmployee_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = BarcodeAllocationBuilders.ValidCreateCommand(empName: "");
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.EmployeeName);
        }

        [Fact]
        public async Task Validate_ToNotGreaterThanFrom_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = BarcodeAllocationBuilders.ValidCreateCommand(from: 5000, to: 1000);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.BarcodeTo);
        }

        [Fact]
        public async Task Validate_OverlappingRange_FailsValidation()
        {
            SetupAllAsyncMocks(overlaps: true);
            var result = await CreateValidator().TestValidateAsync(BarcodeAllocationBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor("Barcode Range");
        }

        [Fact]
        public async Task Validate_RangeOutsideSeries_FailsValidation()
        {
            SetupAllAsyncMocks(within: false);
            var result = await CreateValidator().TestValidateAsync(BarcodeAllocationBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor("Barcode Range");
        }
    }
}
