using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent;
using SalesManagement.Presentation.Validation.OfficerAgent;

namespace SalesManagement.UnitTests.Validators.OfficerAgent
{
    public sealed class UpdateOfficerAgentCommandValidatorTests
    {
        private readonly Mock<IOfficerAgentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateOfficerAgentCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int marketingOfficerId = 1, int assignmentId = 10, int agentId = 2)
        {
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(marketingOfficerId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(assignmentId)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsExpiredAsync(assignmentId)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(agentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        private static UpdateOfficerAgentCommand ValidCommand() => new()
        {
            MarketingOfficerId = 1,
            Agents = new List<OfficerAgentUpdateItem>
            {
                new()
                {
                    Id = 10,
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
            _mockQueryRepo.Setup(r => r.NotFoundAsync(10)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsExpiredAsync(10)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.MarketingOfficerId);
        }

        // ── Agents List Rules ─────────────────────────────────────────────────

        [Fact]
        public async Task Agents_EmptyList_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Agents = new List<OfficerAgentUpdateItem>();
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Agents);
        }

        [Fact]
        public async Task AssignmentId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Agents[0].Id = 0;
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsExpiredAsync(0)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Assignment_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(10)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsExpiredAsync(10)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Assignment_IsExpired_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(10)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsExpiredAsync(10)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }

        // ── ValidityTo Rules ──────────────────────────────────────────────────

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
    }
}
