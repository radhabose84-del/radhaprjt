using FluentValidation.TestHelper;
using FinanceManagement.Application.AccountGroup.Commands.UpdateAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Presentation.Validation.AccountGroup;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.AccountGroup
{
    public sealed class UpdateAccountGroupCommandValidatorTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateAccountGroupCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static UpdateAccountGroupCommand ValidCommand() =>
            new() { Id = 1, GroupName = "Inventories", IsActive = 1 };

        // Default: entity exists and is a non-L1 node (level 3), so the L1-name rule is skipped.
        private void SetupHappyPath(int level = 3)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetLevelAsync(1)).ReturnsAsync(level);
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyGroupName_Fails()
        {
            SetupHappyPath();
            var command = ValidCommand();
            command.GroupName = "";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }

        [Fact]
        public async Task Validate_IsActiveOutOfRange_Fails()
        {
            SetupHappyPath();
            var command = ValidCommand();
            command.IsActive = 2;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_NotFound_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_Level1RenameToInvalidName_Fails()
        {
            SetupHappyPath(level: 1);
            var command = ValidCommand();
            command.GroupName = "Sundry";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }

        [Fact]
        public async Task Validate_Level1RenameToValidName_Passes()
        {
            SetupHappyPath(level: 1);
            var command = ValidCommand();
            command.GroupName = "Assets";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
