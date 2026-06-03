using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Presentation.Validation.BarcodeSeries;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.BarcodeSeries
{
    public sealed class UpdateBarcodeSeriesCommandValidatorTests
    {
        private readonly Mock<IBarcodeSeriesQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateBarcodeSeriesCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(bool exists = true, bool validPrefix = true, bool overlaps = false, bool allocated = false)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(exists);
            _mockQueryRepo.Setup(r => r.IsValidPrefixAsync(It.IsAny<int>())).ReturnsAsync(validPrefix);
            _mockQueryRepo.Setup(r => r.RangeOverlapsAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int?>())).ReturnsAsync(overlaps);
            _mockQueryRepo.Setup(r => r.IsAllocatedAsync(It.IsAny<int>())).ReturnsAsync(allocated);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = BarcodeSeriesBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            SetupAllAsyncMocks(exists: false);
            var command = BarcodeSeriesBuilders.ValidUpdateCommand(id: 99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = BarcodeSeriesBuilders.ValidUpdateCommand(isActive: 5);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_AllocatedSeries_FailsValidation()
        {
            SetupAllAsyncMocks(allocated: true);
            var command = BarcodeSeriesBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_OverlappingRange_FailsValidation()
        {
            SetupAllAsyncMocks(overlaps: true);
            var command = BarcodeSeriesBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("Barcode Range");
        }
    }
}
