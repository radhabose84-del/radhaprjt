using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Commands.DeleteMovementTypeConfig;
using SalesManagement.Presentation.Validation.MovementTypeConfig;

namespace SalesManagement.UnitTests.Validators.MovementTypeConfig
{
    public sealed class DeleteMovementTypeConfigCommandValidatorTests
    {
        private readonly Mock<IMovementTypeConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMovementTypeConfigCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(false);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(new DeleteMovementTypeConfigCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ── Id / NotEmpty Rules ───────────────────────────────────────────────

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMovementTypeConfigCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        // ── NotFound Rules ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMovementTypeConfigCommand(1));

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task EntityExists_PassesNotFoundCheck()
        {
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(new DeleteMovementTypeConfigCommand(1));

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }
    }
}
