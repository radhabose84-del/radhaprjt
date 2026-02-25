#nullable disable
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
    /// Individual tests then override specific setups to trigger the desired failure.
    /// </summary>
    public class CreateSalesItemPriceMasterCommandValidatorTests
    {
        private readonly Mock<ISalesItemPriceMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateSalesItemPriceMasterCommandValidator CreateValidator()
            => new CreateSalesItemPriceMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PaymentTermExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CurrencyExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.OverlapExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── PriceCode Rules ───────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task PriceCode_Empty_FailsValidation(string code)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(priceCode: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PriceCode)
                  .WithErrorMessage("PriceCode is required.");
        }

        [Fact]
        public async Task PriceCode_TooLong_FailsValidation()
        {
            SetupAllValid();
            var longCode = new string('A', 21);
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(priceCode: longCode);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PriceCode)
                  .WithErrorMessage("PriceCode  cannot be longer than   20 characters.");
        }

        [Fact]
        public async Task PriceCode_MaxLength20_PassesValidation()
        {
            SetupAllValid();
            var maxCode = new string('A', 20);
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(priceCode: maxCode);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.PriceCode);
        }

        [Theory]
        [InlineData("PC 001")]
        [InlineData("PC-001")]
        [InlineData("PC@001")]
        public async Task PriceCode_NonAlphanumeric_FailsValidation(string code)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(priceCode: code);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PriceCode)
                  .WithErrorMessage("PriceCode  must be alphanumeric only.");
        }

        [Fact]
        public async Task PriceCode_AlreadyExists_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync("PC001", It.IsAny<int?>())).ReturnsAsync(true);
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(priceCode: "PC001");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PriceCode)
                  .WithErrorMessage("PriceCode already exists.");
        }

        // ── ItemId Rules ──────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task ItemId_ZeroOrNegative_FailsValidation(int itemId)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(itemId: itemId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ItemId)
                  .WithErrorMessage("ItemId is required.");
        }

        [Fact]
        public async Task ItemId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(itemId: 10);

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
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(salesSegmentId: segmentId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesSegmentId)
                  .WithErrorMessage("SalesSegmentId is required.");
        }

        [Fact]
        public async Task SalesSegmentId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(1)).ReturnsAsync(false);
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(salesSegmentId: 1);

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
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(paymentTermsId: ptId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PaymentTermsId)
                  .WithErrorMessage("PaymentTermsId is required.");
        }

        [Fact]
        public async Task PaymentTermsId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.PaymentTermExistsAsync(2)).ReturnsAsync(false);
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(paymentTermsId: 2);

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
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(exMillPrice: price);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ExMillPrice)
                  .WithErrorMessage("ExMillPrice must be greater than zero.");
        }

        // ── CurrencyId Rules ──────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        public async Task CurrencyId_ZeroOrNegative_FailsValidation(int currencyId)
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(currencyId: currencyId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId)
                  .WithErrorMessage("CurrencyId is required.");
        }

        [Fact]
        public async Task CurrencyId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.CurrencyExistsAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(currencyId: 5);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CurrencyId)
                  .WithErrorMessage("CurrencyId Currency Id is inactive/deleted.");
        }

        // ── ValidFrom / ValidTo Rules ─────────────────────────────────────────

        [Fact]
        public async Task ValidFrom_DefaultValue_FailsValidation()
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(validFrom: default(DateTimeOffset));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ValidFrom)
                  .WithErrorMessage("ValidFrom is required.");
        }

        [Fact]
        public async Task ValidTo_DefaultValue_FailsValidation()
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(validTo: default(DateTimeOffset));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ValidTo);
        }

        [Fact]
        public async Task ValidTo_BeforeValidFrom_FailsValidation()
        {
            SetupAllValid();
            var validFrom = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var validTo   = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero); // before validFrom
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand(validFrom: validFrom, validTo: validTo);

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
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            var command = SalesItemPriceMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError()
                  .WithErrorMessage("An active price record already exists.");
        }

        [Fact]
        public async Task Overlap_NotExists_PassesValidation()
        {
            SetupAllValid();
            var command = SalesItemPriceMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
