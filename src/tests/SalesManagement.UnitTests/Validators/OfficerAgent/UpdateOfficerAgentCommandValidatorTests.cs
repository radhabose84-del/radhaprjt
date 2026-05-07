using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IOfficerAgent;
using SalesManagement.Application.OfficerAgent.Commands.UpdateOfficerAgent;
using SalesManagement.Presentation.Validation.OfficerAgent;

namespace SalesManagement.UnitTests.Validators.OfficerAgent
{
    public sealed class UpdateOfficerAgentCommandValidatorTests
    {
        private readonly Mock<IOfficerAgentQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

        private UpdateOfficerAgentCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object, _mockAccessFilter.Object);

        private void SetupAllAsyncMocks(int marketingOfficerId = 1, int assignmentId = 10, int agentId = 2)
        {
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(marketingOfficerId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(assignmentId)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(agentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyAssignedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsExpiredAndModifiedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
                .ReturnsAsync(false);
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
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyAssignedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsExpiredAndModifiedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
                .ReturnsAsync(false);

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
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyAssignedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsExpiredAndModifiedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Assignment_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(10)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyAssignedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsExpiredAndModifiedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }

        // ── SCRUM-1559: expired-row check now only fires when row is actually modified ─────

        [Fact]
        public async Task Assignment_ExpiredAndModified_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(10)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyAssignedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            // Row is expired AND user changed something
            _mockQueryRepo.Setup(r => r.IsExpiredAndModifiedAsync(
                10, 2, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), 1))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor("Agents[0]")
                  .WithErrorMessage("Expired assignments cannot be edited.");
        }

        [Fact]
        public async Task Assignment_ExpiredButUnchanged_PassesValidation()
        {
            // Round-trip case: expired row in payload that the user did NOT modify must not block save.
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(10)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyAssignedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            // Row may be expired in DB, but values match — repo returns false (not modified)
            _mockQueryRepo.Setup(r => r.IsExpiredAndModifiedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
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

        // ── Duplicate Rules (SCRUM-1561) ──────────────────────────────────────

        [Fact]
        public async Task SameAgentListedTwice_InOnePayload_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.Agents.Add(new OfficerAgentUpdateItem
            {
                Id = 11,
                AgentId = 2, // duplicate of the first item
                ValidityFrom = DateOnly.FromDateTime(DateTime.Today),
                ValidityTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
                IsActive = 1
            });
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyAssignedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsExpiredAndModifiedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Agents)
                  .WithErrorMessage("Agent appears more than once in this request.");
        }

        [Fact]
        public async Task AgentAlreadyActivelyAssigned_OnAnotherRow_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.MarketingOfficerExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(10)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            // Active row exists for (officer=1, agent=2) other than this row (Id=10)
            _mockQueryRepo.Setup(r => r.AlreadyAssignedAsync(1, 2, 10)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsExpiredAndModifiedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor("Agents[0]")
                  .WithErrorMessage("This agent is already assigned to this Marketing Officer.");
        }

        [Fact]
        public async Task EditOwnRow_DateChangeOnly_PassesValidation()
        {
            // Editing the same row's date range should NOT trigger duplicate check against itself.
            var cmd = ValidCommand();
            cmd.Agents[0].ValidityTo = DateOnly.FromDateTime(DateTime.Today.AddYears(1));
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
            // Verify excludeAssignmentId was passed correctly
            _mockQueryRepo.Verify(r => r.AlreadyAssignedAsync(1, 2, 10), Times.AtLeastOnce);
        }
    }
}
