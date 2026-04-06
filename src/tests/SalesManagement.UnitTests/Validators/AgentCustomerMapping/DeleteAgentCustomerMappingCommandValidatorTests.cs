using FluentValidation.TestHelper;
using SalesManagement.Application.AgentCustomerMapping.Commands.DeleteAgentCustomerMapping;
using SalesManagement.Application.Common.Interfaces.IAgentCustomerMapping;
using SalesManagement.Presentation.Validation.AgentCustomerMapping;

namespace SalesManagement.UnitTests.Validators.AgentCustomerMapping
{
    public sealed class DeleteAgentCustomerMappingCommandValidatorTests
    {
        private readonly Mock<IAgentCustomerMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteAgentCustomerMappingCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(new DeleteAgentCustomerMappingCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id / NotEmpty Rules ───────────────────────────────────────────────

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(0, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteAgentCustomerMappingCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── NotFound Rules ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteAgentCustomerMappingCommand(1));

            result.ShouldHaveAnyValidationError();
        }

        // ── SoftDelete Rules ──────────────────────────────────────────────────

        [Fact]
        public async Task HasDependencies_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteAgentCustomerMappingCommand(1));

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task NoDependencies_PassesValidation()
        {
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(new DeleteAgentCustomerMappingCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
