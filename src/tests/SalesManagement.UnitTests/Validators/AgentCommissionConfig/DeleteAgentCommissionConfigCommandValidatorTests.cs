using FluentValidation.TestHelper;
using SalesManagement.Application.AgentCommissionConfig.Commands.DeleteAgentCommissionConfig;
using SalesManagement.Application.Common.Interfaces.IAgentCommissionConfig;
using SalesManagement.Presentation.Validation.AgentCommissionConfig;

namespace SalesManagement.UnitTests.Validators.AgentCommissionConfig
{
    public sealed class DeleteAgentCommissionConfigCommandValidatorTests
    {
        private readonly Mock<IAgentCommissionConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteAgentCommissionConfigCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidId_ExistingEntity_PassesValidation()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteAgentCommissionConfigCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id / NotEmpty Rules ───────────────────────────────────────────────

        [Fact]
        public async Task Id_Zero_FailsNotEmpty()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteAgentCommissionConfigCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Id is required.");
        }

        // ── NotFound Rules ────────────────────────────────────────────────────

        [Fact]
        public async Task Id_EntityNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteAgentCommissionConfigCommand(999));

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Agent Commission Configuration not found.");
        }

        [Fact]
        public async Task Id_EntityExists_PassesNotFoundCheck()
        {
            SetupHappyPath(1);

            var result = await CreateValidator().TestValidateAsync(new DeleteAgentCommissionConfigCommand(1));

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
