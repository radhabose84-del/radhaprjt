using FluentValidation.TestHelper;
using SalesManagement.Application.AgentCustomerMapping.Commands.UpdateAgentCustomerMapping;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Presentation.Validation.AgentCustomerMapping;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.AgentCustomerMapping
{
    public sealed class UpdateAgentCustomerMappingCommandValidatorTests
    {
        private readonly Mock<IAgentCustomerMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

        private UpdateAgentCustomerMappingCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockAccessFilter.Object);

        private void SetupAllAsyncMocks(int id = 1, int customerId = 3, int agentId = 2, int salesGroupId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(customerId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(agentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(salesGroupId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockAccessFilter.Setup(f => f.CanAccessAgentAsync(agentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        private static UpdateAgentCustomerMappingCommand ValidCommand() => new()
        {
            Id = 1,
            CustomerId = 3,
            AgentId = 2,
            SalesGroupId = 1,
            EffectiveFrom = DateTime.Today.AddDays(-1),
            IsActive = 1
        };

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id Rules ─────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var cmd = ValidCommand();
            cmd.Id = id;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Id_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── CustomerId Rules ──────────────────────────────────────────────────

        [Fact]
        public async Task CustomerId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.CustomerId = 0;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        }

        [Fact]
        public async Task CustomerId_NotFound_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.CustomerId = 999;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.CustomerId);
        }

        // ── AgentId Rules ─────────────────────────────────────────────────────

        [Fact]
        public async Task AgentId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.AgentId = 0;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SalesGroupExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.AgentId);
        }

        // ── SalesGroupId Rules ──────────────────────────────────────────────

        [Fact]
        public async Task SalesGroupId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SalesGroupId = 0;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.CustomerExistsAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AgentExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.SalesGroupId);
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

        // ── IsActive Rules ────────────────────────────────────────────────────

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task IsActive_ValidValues_PassesValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldNotHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
