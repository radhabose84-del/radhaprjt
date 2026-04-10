using FluentValidation.TestHelper;
using SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig;
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
    public class CreateAgentCommissionConfigCommandValidatorTests
    {
        private readonly Mock<IAgentCommissionConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateAgentCommissionConfigCommandValidator CreateValidator()
            => new CreateAgentCommissionConfigCommandValidator(
                TestMaxLengthProviderFactory.Create(),
                _mockQueryRepo.Object);

        // ── Setup helpers ─────────────────────────────────────────────────────

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CommissionSplitExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PaymentTermExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.OverlapExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset?>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── AgentId Rules ─────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AgentId_ZeroOrNegative_FailsValidation(int agentId)
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(agentId: agentId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AgentId);
        }

        [Fact]
        public async Task AgentId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(agentId: 10);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AgentId);
        }

        // ── CommissionTypeId Rules ────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CommissionTypeId_ZeroOrNegative_FailsValidation(int typeId)
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(commissionTypeId: typeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CommissionTypeId);
        }

        [Fact]
        public async Task CommissionTypeId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(40)).ReturnsAsync(false);
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(commissionTypeId: 40);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CommissionTypeId);
        }

        // ── CommissionSplitId Rules ───────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CommissionSplitId_ZeroOrNegative_FailsValidation(int splitId)
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(commissionSplitId: splitId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CommissionSplitId);
        }

        [Fact]
        public async Task CommissionSplitId_NotFound_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.CommissionSplitExistsAsync(110)).ReturnsAsync(false);
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(commissionSplitId: 110);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CommissionSplitId);
        }

        // ── CommissionPercentage Rules ────────────────────────────────────────

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task CommissionPercentage_NotPositive_FailsValidation(decimal pct)
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(commissionPercentage: pct);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CommissionPercentage);
        }

        // ── ValidityFrom / ValidityTo Rules ──────────────────────────────────

        [Fact]
        public async Task ValidityFrom_DefaultValue_FailsValidation()
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(validityFrom: default(DateTimeOffset));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ValidityFrom);
        }

        [Fact]
        public async Task ValidityTo_BeforeValidityFrom_FailsValidation()
        {
            SetupAllValid();
            var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to   = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(validityFrom: from, validityTo: to);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ValidityTo)
                  .WithErrorMessage("ValidityTo must be greater than or equal to ValidityFrom.");
        }

        // ── Slabs Rules ───────────────────────────────────────────────────────

        [Fact]
        public async Task Slabs_Null_FailsValidation()
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(slabs: null);
            command.Slabs = null;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Slabs);
        }

        [Fact]
        public async Task Slabs_Empty_FailsValidation()
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidCreateCommand(slabs: new List<AgentCommissionSlabItem>());

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Slabs);
        }

        // ── Overlap Rules ─────────────────────────────────────────────────────

        [Fact]
        public async Task Overlap_Exists_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.OverlapExistsAsync(
                    It.IsAny<int>(), It.IsAny<int>(),
                    It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset?>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            var command = AgentCommissionConfigBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError()
                  .WithErrorMessage("An active commission rule already exists for this Agent and Commission Split within the specified validity period.");
        }

        [Fact]
        public async Task Overlap_NotExists_PassesValidation()
        {
            SetupAllValid();
            var command = AgentCommissionConfigBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
