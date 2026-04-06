using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.DeleteRoleItemGroupMapping;
using UserManagement.Presentation.Validation.RoleItemGroupMapping;

namespace UserManagement.UnitTests.Validators.RoleItemGroupMapping
{
    public sealed class DeleteRoleItemGroupMappingCommandValidatorTests
    {
        private readonly Mock<IRoleItemGroupMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteRoleItemGroupMappingCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteRoleItemGroupMappingCommand { Id = 1 });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteRoleItemGroupMappingCommand { Id = 0 });
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteRoleItemGroupMappingCommand { Id = 1 });
            result.ShouldHaveAnyValidationError();
        }
    }
}
