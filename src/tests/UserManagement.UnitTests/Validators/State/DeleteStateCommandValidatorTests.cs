using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.State.Commands.DeleteState;
using UserManagement.Presentation.Validation.State;

namespace UserManagement.UnitTests.Validators.State
{
    public sealed class DeleteStateCommandValidatorTests
    {
        private readonly Mock<IStateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteStateCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteStateCommand { Id = 1 });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteStateCommand { Id = 0 });
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task LinkedRecords_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteStateCommand { Id = 1 });
            result.ShouldHaveAnyValidationError();
        }
    }
}
