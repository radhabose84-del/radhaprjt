using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IUserRole;
using UserManagement.Application.UserRole.Commands.DeleteRole;
using UserManagement.Presentation.Validation.UserRole;

namespace UserManagement.UnitTests.Validators.UserRole
{
    public sealed class DeleteRoleCommandValidatorTests
    {
        private readonly Mock<IUserRoleQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteRoleCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteRoleCommand { Id = 1 });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(0)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteRoleCommand { Id = 0 });
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task LinkedRecords_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteRoleCommand { Id = 1 });
            result.ShouldHaveAnyValidationError();
        }
    }
}
