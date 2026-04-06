using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IUserGroup;
using UserManagement.Application.UserGroup.Commands.DeleteUserGroup;
using UserManagement.Presentation.Validation.UserGroup;

namespace UserManagement.UnitTests.Validators.UserGroup
{
    public sealed class DeleteUserGroupCommandValidatorTests
    {
        private readonly Mock<IUserGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteUserGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteUserGroupCommand { Id = 1 });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteUserGroupCommand { Id = 0 });
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task LinkedRecords_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteUserGroupCommand { Id = 1 });
            result.ShouldHaveAnyValidationError();
        }
    }
}
