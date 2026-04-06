using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.CreateRoleItemGroupMapping;
using UserManagement.Presentation.Validation.RoleItemGroupMapping;

namespace UserManagement.UnitTests.Validators.RoleItemGroupMapping
{
    public sealed class CreateRoleItemGroupMappingCommandValidatorTests
    {
        private readonly Mock<IRoleItemGroupMappingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private CreateRoleItemGroupMappingCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object);

        private static CreateRoleItemGroupMappingCommand ValidCommand() =>
            new CreateRoleItemGroupMappingCommand { RoleId = 1, ItemGroupId = 2 };

        private void SetupHappyPath(int roleId = 1, int itemGroupId = 2)
        {
            _mockCommandRepo.Setup(r => r.CompositeKeyExistsAsync(roleId, itemGroupId, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task RoleId_ZeroOrNegative_FailsValidation(int roleId)
        {
            var cmd = ValidCommand();
            cmd.RoleId = roleId;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.RoleId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task ItemGroupId_ZeroOrNegative_FailsValidation(int itemGroupId)
        {
            var cmd = ValidCommand();
            cmd.ItemGroupId = itemGroupId;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ItemGroupId);
        }

        [Fact]
        public async Task DuplicateCompositeKey_FailsValidation()
        {
            _mockCommandRepo.Setup(r => r.CompositeKeyExistsAsync(1, 2, null)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
