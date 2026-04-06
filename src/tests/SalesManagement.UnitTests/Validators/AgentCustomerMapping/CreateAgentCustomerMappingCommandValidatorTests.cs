using FluentValidation.TestHelper;
using SalesManagement.Application.AgentCustomerMapping.Commands.CreateAgentCustomerMapping;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Presentation.Validation.AgentCustomerMapping;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.AgentCustomerMapping
{
    public sealed class CreateAgentCustomerMappingCommandValidatorTests
    {
        private readonly Mock<IAgentCustomerMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateAgentCustomerMappingCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int customerId = 1, int agentId = 2, int salesSegmentId = 1)
        {
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(agentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(salesSegmentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        private static CreateAgentCustomerMappingCommand ValidCommand() => new()
        {
            CustomerId = 1,
            AgentId = 2,
            SalesSegmentId = 1,
            EffectiveFrom = DateTime.Today.AddDays(-1)
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var cmd = ValidCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── CustomerId Rules ──────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CustomerId_ZeroOrNegative_FailsValidation(int customerId)
        {
            var cmd = ValidCommand();
            cmd.CustomerId = customerId;
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        }

        [Fact]
        public async Task CustomerId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        }

        // ── AgentId Rules ─────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task AgentId_ZeroOrNegative_FailsValidation(int agentId)
        {
            var cmd = ValidCommand();
            cmd.AgentId = agentId;
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.AgentId);
        }

        // ── SalesSegmentId Rules ──────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SalesSegmentId_ZeroOrNegative_FailsValidation(int segId)
        {
            var cmd = ValidCommand();
            cmd.SalesSegmentId = segId;
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.SalesSegmentId);
        }

        [Fact]
        public async Task SalesSegmentId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesSegmentExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.SalesSegmentId);
        }

        // ── EffectiveFrom Rules ───────────────────────────────────────────────

        [Fact]
        public async Task EffectiveFrom_FutureDate_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.EffectiveFrom = DateTime.Today.AddDays(5);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.EffectiveFrom);
        }

        // ── SubAgentId Rules ──────────────────────────────────────────────────

        [Fact]
        public async Task SubAgentId_SameAsAgentId_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SubAgentId = cmd.AgentId;
            SetupAllAsyncMocks();
            _mockQueryRepo.Setup(r => r.SubAgentExistsAsync(cmd.AgentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot be the same as AgentId"));
        }

        // ── EffectiveTo Rules ─────────────────────────────────────────────────

        [Fact]
        public async Task EffectiveTo_BeforeEffectiveFrom_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.EffectiveFrom = DateTime.Today.AddDays(-5);
            cmd.EffectiveTo = DateTime.Today.AddDays(-10);
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.EffectiveTo);
        }
    }
}
