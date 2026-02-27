using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Presentation.Validation.SalesItemPriceMaster;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesItemPriceMaster
{
    /// <summary>
    /// FluentValidation runs ALL rules regardless of earlier failures.
    /// SetupAllValid() must be called as a baseline in every test to satisfy MockBehavior.Strict.
    /// Note: PriceCode is IMMUTABLE — it is not included in UpdateCommand.
    /// </summary>
    public class UpdateSalesItemPriceMasterCommandValidatorTests
    {
        private readonly Mock<ISalesItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateSalesItemPriceMasterCommandValidator CreateValidator()
            => new UpdateSalesItemPriceMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PaymentTermExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CurrencyExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.OverlapExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ──────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(id: id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Sales Item Price Master Id is required.");
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(id: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Sales Item Price Master not found.");
        }

        // ── ItemId Rules ──────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task ItemId_ZeroOrNegative_FailsValidation(int itemId)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(itemId: itemId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ItemId)
                  .WithErrorMessage("ItemId is required.");
        }

        [Fact]
        public async Task ItemId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(itemId: 10);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ItemId)
                  .WithErrorMessage("ItemId Item Id is inactive/deleted.");
        }

        // ── SalesSegmentId Rules ──────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task SalesSegmentId_ZeroOrNegative_FailsValidation(int segmentId)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(salesSegmentId: segmentId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesSegmentId)
                  .WithErrorMessage("SalesSegmentId is required.");
        }

        [Fact]
        public async Task SalesSegmentId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(1)).ReturnsAsync(false);
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(salesSegmentId: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesSegmentId)
                  .WithErrorMessage("SalesSegmentId Sales Segment Id is inactive/deleted.");
        }

        // ── PaymentTermsId Rules ──────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task PaymentTermsId_ZeroOrNegative_FailsValidation(int ptId)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(paymentTermsId: ptId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PaymentTermsId)
                  .WithErrorMessage("PaymentTermsId is required.");
        }

        [Fact]
        public async Task PaymentTermsId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.PaymentTermExistsAsync(2)).ReturnsAsync(false);
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(paymentTermsId: 2);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PaymentTermsId)
                  .WithErrorMessage("PaymentTermsId Payment Terms Id is inactive/deleted.");
        }

        // ── ExMillPrice Rules ─────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task ExMillPrice_ZeroOrNegative_FailsValidation(decimal price)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(exMillPrice: price);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ExMillPrice)
                  .WithErrorMessage("ExMillPrice must be greater than zero.");
        }

        // ── CurrencyId Rules ──────────────────────────────────────────────────

        [Fact]
        public async Task CurrencyId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.CurrencyExistsAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(currencyId: 5);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId)
                  .WithErrorMessage("CurrencyId Currency Id is inactive/deleted.");
        }

        // ── ValidFrom / ValidTo Rules ─────────────────────────────────────────

        [Fact]
        public async Task ValidTo_BeforeValidFrom_FailsValidation()
        {
            SetupAllValid();
            var validFrom = new DateOnly(2025, 6, 1);
            var validTo   = new DateOnly(2025, 1, 1);
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(validFrom: validFrom, validTo: validTo);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ValidTo)
                  .WithErrorMessage("Valid To must be after Valid From.");
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(2)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(isActive: isActive);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive)
                  .WithErrorMessage("IsActive  must be either 0 or 1.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassValidation(int isActive)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand(isActive: isActive);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        // ── Overlap Rules (self-excluded) ─────────────────────────────────────

        [Fact]
        public async Task Overlap_ExistsForAnotherRecord_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.OverlapExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            var command = SalesItemPriceMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError()
                  .WithErrorMessage("An active price record already exists.");
        }
    }
}
