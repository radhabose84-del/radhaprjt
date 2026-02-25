#nullable disable
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment;
using SalesManagement.Presentation.Validation.SalesSegment;
using Contracts.Interfaces.Lookups.Users;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesSegment
{
    /// <summary>
    /// ⚠️ Update validator has NotFoundAsync check on Id + IsActive validation (0 or 1).
    /// Composite key fields (SalesOrganisationId, SalesChannelId, BusinessUnitId) are IMMUTABLE
    /// and must NOT appear in the UpdateSalesSegmentCommand.
    /// </summary>
    public class UpdateSalesSegmentCommandValidatorTests
    {
        private readonly Mock<ISalesSegmentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);

        private UpdateSalesSegmentCommandValidator CreateValidator()
            => new UpdateSalesSegmentCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object,
                _mockCurrencyLookup.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupIdExists(int id = 1)
        {
            // NotFoundAsync returns false when entity IS found
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        private void SetupIdNotFound(int id = 99)
        {
            // NotFoundAsync returns true when entity is NOT found
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupIdExists(1);
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ─────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            // NotFoundAsync still fires for chained rule — even when GreaterThan fails
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Sales Segment Id is required.");
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            SetupIdNotFound(99);
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: 99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Sales Segment not found.");
        }

        // ── SegmentName Rules ─────────────────────────────────────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task SegmentName_Empty_FailsValidation(string name)
        {
            SetupIdExists(1);
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: 1, segmentName: name);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SegmentName)
                  .WithErrorMessage("SegmentName is required.");
        }

        [Fact]
        public async Task SegmentName_TooLong_FailsValidation()
        {
            SetupIdExists(1);
            var longName = new string('A', 201);
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: 1, segmentName: longName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SegmentName)
                  .WithErrorMessage("SegmentName  cannot be longer than   200 characters.");
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassesValidation(int isActive)
        {
            SetupIdExists(1);
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: 1, isActive: isActive);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            SetupIdExists(1);
            var command = SalesSegmentBuilders.ValidUpdateCommand(id: 1, isActive: isActive);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive)
                  .WithErrorMessage("IsActive  must be either 0 or 1.");
        }

        // ── Immutability Verification ─────────────────────────────────────────

        [Fact]
        public void UpdateCommand_DoesNotContain_SalesOrganisationId_Property()
        {
            var commandType = typeof(UpdateSalesSegmentCommand);
            var prop = commandType.GetProperty("SalesOrganisationId");

            prop.Should().BeNull(
                "SalesOrganisationId is immutable and must NOT be included in UpdateSalesSegmentCommand");
        }

        [Fact]
        public void UpdateCommand_DoesNotContain_SalesChannelId_Property()
        {
            var commandType = typeof(UpdateSalesSegmentCommand);
            var prop = commandType.GetProperty("SalesChannelId");

            prop.Should().BeNull(
                "SalesChannelId is immutable and must NOT be included in UpdateSalesSegmentCommand");
        }

        [Fact]
        public void UpdateCommand_DoesNotContain_BusinessUnitId_Property()
        {
            var commandType = typeof(UpdateSalesSegmentCommand);
            var prop = commandType.GetProperty("BusinessUnitId");

            prop.Should().BeNull(
                "BusinessUnitId is immutable and must NOT be included in UpdateSalesSegmentCommand");
        }
    }
}
