using FluentValidation.TestHelper;
using FinanceManagement.Application.AccountGroup.Commands.CreateAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Presentation.Validation.AccountGroup;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.AccountGroup
{
    public sealed class CreateAccountGroupCommandValidatorTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateAccountGroupCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        // Valid non-root command (created under an existing non-leaf parent).
        private static CreateAccountGroupCommand ValidChildCommand() =>
            new() { CompanyId = 1, GroupCode = "A-CA-INV-FF", GroupName = "Finished Goods — Fabric", ParentAccountGroupId = 5, SortOrder = 1 };

        private void SetupHappyChild(int parentId = 5, int parentLevel = 3)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ParentExistsAsync(parentId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetLevelAsync(parentId)).ReturnsAsync(parentLevel);
        }

        [Fact]
        public async Task Validate_ValidChildCommand_Passes()
        {
            SetupHappyChild();
            var result = await CreateValidator().TestValidateAsync(ValidChildCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyGroupCode_Fails()
        {
            SetupHappyChild();
            var command = ValidChildCommand();
            command.GroupCode = "";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupCode);
        }

        [Fact]
        public async Task Validate_EmptyGroupName_Fails()
        {
            SetupHappyChild();
            var command = ValidChildCommand();
            command.GroupName = "";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }

        [Fact]
        public async Task Validate_DuplicateGroupCode_Fails()
        {
            SetupHappyChild();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidChildCommand());
            result.ShouldHaveValidationErrorFor(x => x.GroupCode);
        }

        [Fact]
        public async Task Validate_ZeroCompanyId_Fails()
        {
            SetupHappyChild();
            var command = ValidChildCommand();
            command.CompanyId = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.CompanyId);
        }

        [Fact]
        public async Task Validate_ParentDoesNotExist_Fails()
        {
            SetupHappyChild();
            _mockQueryRepo.Setup(r => r.ParentExistsAsync(5)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidChildCommand());
            result.ShouldHaveValidationErrorFor(x => x.ParentAccountGroupId);
        }

        [Fact]
        public async Task Validate_ParentAtMaxDepth_Fails()
        {
            SetupHappyChild(parentLevel: 4);
            var result = await CreateValidator().TestValidateAsync(ValidChildCommand());
            result.ShouldHaveValidationErrorFor(x => x.ParentAccountGroupId);
        }

        [Fact]
        public async Task Validate_Level1ValidName_Passes()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            var command = new CreateAccountGroupCommand { CompanyId = 1, GroupCode = "A", GroupName = "Assets", ParentAccountGroupId = null, SortOrder = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_Level1InvalidName_Fails()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            var command = new CreateAccountGroupCommand { CompanyId = 1, GroupCode = "X", GroupName = "Sundry", ParentAccountGroupId = null, SortOrder = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }
    }
}
