using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.CreateOfficerAgent;
using SalesManagement.Presentation.Validation.OfficerAgent;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.OfficerAgent
{
    public sealed class CreateOfficerAgentCommandValidatorTests
    {
        private readonly Mock<IOfficerAgentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

        private CreateOfficerAgentCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockAccessFilter.Object);

        private void SetupAllAsyncMocks(int marketingOfficerId = 1, int agentId = 2)
        {
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(marketingOfficerId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(agentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        private static CreateOfficerAgentCommand ValidCommand() => new()
        {
            MarketingOfficerId = 1,
            Agents = new List<OfficerAgentBatchItem>
            {
                new()
                {
                    AgentId = 2,
                    ValidityFrom = DateOnly.FromDateTime(DateTime.Today),
                    ValidityTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
                    IsActive = 1
                }
            }
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── MarketingOfficerId Rules ───────────────────────────────────────────

        [Fact]
        public async Task MarketingOfficerId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.MarketingOfficerId = 0;
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MarketingOfficerId);
        }

        [Fact]
        public async Task MarketingOfficerId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MarketingOfficerId);
        }

        // ── Agents List Rules ─────────────────────────────────────────────────

        [Fact]
        public async Task Agents_EmptyList_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Agents = new List<OfficerAgentBatchItem>();
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Agents);
        }

        [Fact]
        public async Task AgentId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Agents[0].AgentId = 0;
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task ValidityTo_BeforeValidityFrom_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Agents[0].ValidityFrom = DateOnly.FromDateTime(DateTime.Today.AddDays(10));
            cmd.Agents[0].ValidityTo = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task ValidityTo_PastDate_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Agents[0].ValidityFrom = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            cmd.Agents[0].ValidityTo = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task AgentIsActive_InvalidValue_FailsValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.Agents[0].IsActive = isActive;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }
    }
}
