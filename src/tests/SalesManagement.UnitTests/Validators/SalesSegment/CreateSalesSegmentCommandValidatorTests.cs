#nullable disable
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment;
using SalesManagement.Presentation.Validation.SalesSegment;
using Contracts.Interfaces.Lookups.Users;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesSegment
{
    /// <summary>
    /// ⚠️ FluentValidation runs ALL rules regardless of earlier failures.
    /// SetupAllValid() must be called as a baseline in every test to satisfy MockBehavior.Strict.
    /// Individual tests then override specific setups to trigger the desired failure.
    /// </summary>
    public class CreateSalesSegmentCommandValidatorTests
    {
        private readonly Mock<ISalesSegmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);

        private CreateSalesSegmentCommandValidator CreateValidator()
            => new CreateSalesSegmentCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockCurrencyLookup.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Sets up all FK existence checks and composite key check to pass (all valid).
        /// Use It.IsAny so exact arg values don't need to match in "invalid ID" scenarios.
        /// </summary>
        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.SalesOrganisationExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesChannelExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.BusinessUnitExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var command = SalesSegmentBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── SalesOrganisationId Rules ─────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SalesOrganisationId_ZeroOrNegative_FailsValidation(int orgId)
        {
            SetupAllValid();
            var command = SalesSegmentBuilders.ValidCreateCommand(salesOrganisationId: orgId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationId)
                  .WithErrorMessage("Sales Organisation is required.");
        }

        [Fact]
        public async Task SalesOrganisationId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.SalesOrganisationExistsAsync(1)).ReturnsAsync(false);
            var command = SalesSegmentBuilders.ValidCreateCommand(salesOrganisationId: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesOrganisationId)
                  .WithErrorMessage("Sales Organisation does not exist in Sales Organisation Master.");
        }

        // ── SalesChannelId Rules ──────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SalesChannelId_ZeroOrNegative_FailsValidation(int channelId)
        {
            SetupAllValid();
            var command = SalesSegmentBuilders.ValidCreateCommand(salesChannelId: channelId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesChannelId)
                  .WithErrorMessage("Sales Channel is required.");
        }

        [Fact]
        public async Task SalesChannelId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.SalesChannelExistsAsync(1)).ReturnsAsync(false);
            var command = SalesSegmentBuilders.ValidCreateCommand(salesChannelId: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesChannelId)
                  .WithErrorMessage("Sales Channel does not exist in Sales Channel Master.");
        }

        // ── BusinessUnitId Rules ──────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task BusinessUnitId_ZeroOrNegative_FailsValidation(int buId)
        {
            SetupAllValid();
            var command = SalesSegmentBuilders.ValidCreateCommand(businessUnitId: buId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BusinessUnitId)
                  .WithErrorMessage("Business Unit is required.");
        }

        [Fact]
        public async Task BusinessUnitId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.BusinessUnitExistsAsync(1)).ReturnsAsync(false);
            var command = SalesSegmentBuilders.ValidCreateCommand(businessUnitId: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BusinessUnitId)
                  .WithErrorMessage("Business Unit does not exist in Business Unit Master.");
        }

        // ── SegmentName Rules ─────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SegmentName_Empty_FailsValidation(string name)
        {
            SetupAllValid();
            var command = SalesSegmentBuilders.ValidCreateCommand(segmentName: name);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SegmentName)
                  .WithErrorMessage("Segment Name is required.");
        }

        [Fact]
        public async Task SegmentName_TooLong_FailsValidation()
        {
            SetupAllValid();
            var longName = new string('A', 201);
            var command = SalesSegmentBuilders.ValidCreateCommand(segmentName: longName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SegmentName)
                  .WithErrorMessage("Segment Name cannot exceed 200 characters.");
        }

        [Fact]
        public async Task SegmentName_MaxLength200_PassesValidation()
        {
            SetupAllValid();
            var maxName = new string('A', 200);
            var command = SalesSegmentBuilders.ValidCreateCommand(segmentName: maxName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.SegmentName);
        }

        // ── Composite Key Rules ───────────────────────────────────────────────

        [Fact]
        public async Task CompositeKey_AlreadyExists_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.CompositeKeyExistsAsync(1, 1, 1, It.IsAny<int?>()))
                .ReturnsAsync(true);
            var command = SalesSegmentBuilders.ValidCreateCommand(
                salesOrganisationId: 1, salesChannelId: 1, businessUnitId: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError()
                  .WithErrorMessage("This combination of Sales Organisation, Sales Channel, and Business Unit already exists.");
        }

        [Fact]
        public async Task CompositeKey_Unique_PassesValidation()
        {
            SetupAllValid();
            var command = SalesSegmentBuilders.ValidCreateCommand(
                salesOrganisationId: 1, salesChannelId: 1, businessUnitId: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
