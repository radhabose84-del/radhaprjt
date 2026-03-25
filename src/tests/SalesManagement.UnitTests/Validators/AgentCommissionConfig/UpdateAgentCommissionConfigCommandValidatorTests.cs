using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Presentation.Validation.AgentCommissionConfig;
using SalesManagement.UnitTests.TestData;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.AgentCommissionConfig
{
    /// <summary>
    /// FluentValidation runs ALL rules regardless of earlier failures.
    /// SetupAllValid() must be called as a baseline in every test to satisfy MockBehavior.Strict.
    /// Individual tests then override specific setups to trigger the desired failure.
    /// </summary>
    public class UpdateAgentCommissionConfigCommandValidatorTests
    {
        private readonly Mock<IAgentCommissionConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateAgentCommissionConfigCommandValidator CreateValidator()
            => new UpdateAgentCommissionConfigCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CommissionTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CommissionBasisExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ApplicableLevelExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CurrencyExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.OverlapExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ──────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(id: id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Valid Agent Commission Configuration Id is required.");
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(id: 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── AgentId Rules ─────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AgentId_ZeroOrNegative_FailsValidation(int agentId)
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(agentId: agentId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AgentId);
        }

        [Fact]
        public async Task AgentId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(agentId: 10);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AgentId);
        }

        // ── SalesSegmentId Rules ──────────────────────────────────────────────

        [Fact]
        public async Task SalesSegmentId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(20)).ReturnsAsync(false);
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(salesSegmentId: 20);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.SalesSegmentId);
        }

        // ── CommissionTypeId Rules ────────────────────────────────────────────

        [Fact]
        public async Task CommissionTypeId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.CommissionTypeExistsAsync(40)).ReturnsAsync(false);
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(commissionTypeId: 40);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CommissionTypeId);
        }

        // ── ValidityTo Before ValidityFrom ────────────────────────────────────

        [Fact]
        public async Task ValidityTo_BeforeValidityFrom_FailsValidation()
        {
            SetupAllValid();
            var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to   = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(validityFrom: from, validityTo: to);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ValidityTo)
                  .WithErrorMessage("ValidityTo must be greater than or equal to ValidityFrom.");
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task IsActive_OutOfRange_FailsValidation(int isActive)
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(isActive: isActive);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassesValidation(int isActive)
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidUpdateCommand(isActive: isActive);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }

        // ── Overlap Rules ─────────────────────────────────────────────────────

        [Fact]
        public async Task Overlap_Exists_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.OverlapExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            var command = AgentCommissionConfigBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError()
                  .WithErrorMessage("An active commission rule already exists for this Agent and Sales Segment within the specified validity period.");
        }
    }
}
