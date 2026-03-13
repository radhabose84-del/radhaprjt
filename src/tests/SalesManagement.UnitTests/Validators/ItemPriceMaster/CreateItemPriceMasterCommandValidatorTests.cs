using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Presentation.Validation.ItemPriceMaster;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Validators.ItemPriceMaster
{
    /// <summary>
    /// FluentValidation runs ALL rules regardless of earlier failures.
    /// SetupAllValid() must be called as a baseline in every test to satisfy MockBehavior.Strict.
    /// Individual tests then override specific setups to trigger the desired failure.
    /// PriceCode is auto-generated in the handler — no PriceCode validation rules exist.
    /// </summary>
    public class CreateItemPriceMasterCommandValidatorTests
    {
        private readonly Mock<IItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateItemPriceMasterCommandValidator CreateValidator()
            => new CreateItemPriceMasterCommandValidator(
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CurrencyExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.OverlapExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var command = ItemPriceMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── ItemId Rules ──────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task ItemId_ZeroOrNegative_FailsValidation(int itemId)
        {
            SetupAllValid();
            var command = ItemPriceMasterBuilders.ValidCreateCommand(itemId: itemId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ItemId)
                  .WithErrorMessage("ItemId is required.");
        }

        [Fact]
        public async Task ItemId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var command = ItemPriceMasterBuilders.ValidCreateCommand(itemId: 10);

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
            var command = ItemPriceMasterBuilders.ValidCreateCommand(salesSegmentId: segmentId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesSegmentId)
                  .WithErrorMessage("SalesSegmentId is required.");
        }

        [Fact]
        public async Task SalesSegmentId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(1)).ReturnsAsync(false);
            var command = ItemPriceMasterBuilders.ValidCreateCommand(salesSegmentId: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesSegmentId)
                  .WithErrorMessage("SalesSegmentId Sales Segment Id is inactive/deleted.");
        }

        // ── CurrencyId Rules ──────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task CurrencyId_ZeroOrNegative_FailsValidation(int currencyId)
        {
            SetupAllValid();
            var command = ItemPriceMasterBuilders.ValidCreateCommand(currencyId: currencyId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId)
                  .WithErrorMessage("CurrencyId is required.");
        }

        [Fact]
        public async Task CurrencyId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.CurrencyExistsAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var command = ItemPriceMasterBuilders.ValidCreateCommand(currencyId: 5);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId)
                  .WithErrorMessage("CurrencyId Currency Id is inactive/deleted.");
        }

        // ── ValidFrom / ValidTo Rules ─────────────────────────────────────────

        [Fact]
        public async Task ValidFrom_DefaultValue_FailsValidation()
        {
            SetupAllValid();
            var command = ItemPriceMasterBuilders.ValidCreateCommand(validFrom: default(DateOnly));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ValidFrom)
                  .WithErrorMessage("ValidFrom is required.");
        }

        [Fact]
        public async Task ValidTo_DefaultValue_FailsValidation()
        {
            SetupAllValid();
            var command = ItemPriceMasterBuilders.ValidCreateCommand(validTo: default(DateOnly));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ValidTo);
        }

        [Fact]
        public async Task ValidTo_BeforeValidFrom_FailsValidation()
        {
            SetupAllValid();
            var validFrom = new DateOnly(2025, 6, 1);
            var validTo   = new DateOnly(2025, 1, 1); // before validFrom
            var command = ItemPriceMasterBuilders.ValidCreateCommand(validFrom: validFrom, validTo: validTo);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ValidTo)
                  .WithErrorMessage("Valid To must be after Valid From.");
        }

        // ── Overlap Rules ─────────────────────────────────────────────────────

        [Fact]
        public async Task Overlap_Exists_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.OverlapExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            var command = ItemPriceMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError()
                  .WithErrorMessage("An active price record already exists.");
        }

        [Fact]
        public async Task Overlap_NotExists_PassesValidation()
        {
            SetupAllValid();
            var command = ItemPriceMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
