using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IBarcodeSeries;
using PurchaseManagement.Presentation.Validation.BarcodeSeries;
using PurchaseManagement.UnitTests.TestData;
using PurchaseManagement.UnitTests.TestHelpers;

namespace PurchaseManagement.UnitTests.Validators.BarcodeSeries
{
    public sealed class CreateBarcodeSeriesCommandValidatorTests
    {
        private readonly Mock<IBarcodeSeriesQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateBarcodeSeriesCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(bool validPrefix = true, bool overlaps = false)
        {
            _mockQueryRepo.Setup(r => r.IsValidPrefixAsync(It.IsAny<int>())).ReturnsAsync(validPrefix);
            _mockQueryRepo.Setup(r => r.RangeOverlapsAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int?>())).ReturnsAsync(overlaps);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = BarcodeSeriesBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_MissingPrefix_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = BarcodeSeriesBuilders.ValidCreateCommand(prefixId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PrefixId);
        }

        [Fact]
        public async Task Validate_InvalidPrefix_FailsValidation()
        {
            SetupAllAsyncMocks(validPrefix: false);
            var command = BarcodeSeriesBuilders.ValidCreateCommand(prefixId: 999);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PrefixId);
        }

        [Fact]
        public async Task Validate_StartNumberZero_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = BarcodeSeriesBuilders.ValidCreateCommand(start: 0, end: 5000);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BarcodeStartNumber);
        }

        [Fact]
        public async Task Validate_EndNotGreaterThanStart_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = BarcodeSeriesBuilders.ValidCreateCommand(start: 5000, end: 1000);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BarcodeEndNumber);
        }

        [Fact]
        public async Task Validate_OverlappingRange_FailsValidation()
        {
            SetupAllAsyncMocks(overlaps: true);
            var command = BarcodeSeriesBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor("Barcode Range").WithErrorMessage("Barcode range already exists.");
        }
    }
}
