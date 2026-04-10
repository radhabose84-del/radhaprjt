using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IRoleItemGroupMapping;
using UserManagement.Application.RoleItemGroupMapping.Commands.UpdateRoleItemGroupMapping;
using UserManagement.Presentation.Validation.RoleItemGroupMapping;

namespace UserManagement.UnitTests.Validators.RoleItemGroupMapping
{
    public sealed class UpdateRoleItemGroupMappingCommandValidatorTests
    {
        private readonly Mock<IRoleItemGroupMappingCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRoleItemGroupMappingQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateRoleItemGroupMappingCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object);

        private static UpdateRoleItemGroupMappingCommand ValidCommand() =>
            new UpdateRoleItemGroupMappingCommand { Id = 1, RoleId = 1, ItemGroupId = 2, IsActive = 1 };

        private void SetupHappyPath(int id = 1, int roleId = 1, int itemGroupId = 2)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockCommandRepo.Setup(r => r.CompositeKeyExistsAsync(roleId, itemGroupId, id)).ReturnsAsync(false);
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
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var cmd = ValidCommand();
            cmd.Id = id;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockCommandRepo.Setup(r => r.CompositeKeyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task RoleId_ZeroOrNegative_FailsValidation(int roleId)
        {
            var cmd = ValidCommand();
            cmd.RoleId = roleId;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
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
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ItemGroupId);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockCommandRepo.Setup(r => r.CompositeKeyExistsAsync(1, 2, 1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
