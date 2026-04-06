using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.Units.Commands.DeleteUnit;
using UserManagement.Presentation.Validation.Unit;

namespace UserManagement.UnitTests.Validators.UnitEntity
{
    public sealed class DeleteUnitCommandValidatorTests
    {
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteUnitCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteUnitCommand { UnitId = 1 });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteUnitCommand { UnitId = 0 });
            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }

        [Fact]
        public async Task LinkedRecords_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidationAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteUnitCommand { UnitId = 1 });
            result.ShouldHaveAnyValidationError();
        }
    }
}
