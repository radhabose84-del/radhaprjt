using FluentValidation.TestHelper;
using FinanceManagement.Application.AccountGroup.Commands.CreateAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;
using FinanceManagement.Presentation.Validation.AccountGroup;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.AccountGroup
{
    public sealed class CreateAccountGroupCommandValidatorTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IAccountTypeMasterQueryRepository> _mockAccountTypeRepo = new(MockBehavior.Loose);

        private CreateAccountGroupCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockAccountTypeRepo.Object);

        // Valid non-root command (created under an existing non-leaf parent).
        private static CreateAccountGroupCommand ValidChildCommand() =>
            new() { GroupCode = "A-CA-INV-FF", GroupName = "Finished Goods — Fabric", ParentAccountGroupId = 5, SortOrder = 1 };

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
            // Parent already at the max depth (6) — a child would be Level 7, which is blocked.
            SetupHappyChild(parentLevel: 6);
            var result = await CreateValidator().TestValidateAsync(ValidChildCommand());
            result.ShouldHaveValidationErrorFor(x => x.ParentAccountGroupId);
        }

        [Fact]
        public async Task Validate_ChildWithAccountType_Fails()
        {
            SetupHappyChild();
            var command = ValidChildCommand();
            command.AccountTypeId = 3; // not allowed below Level 1
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.AccountTypeId);
        }

        [Fact]
        public async Task Validate_Level1ValidAccountType_Passes()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            _mockAccountTypeRepo.Setup(r => r.NotFoundAsync(3)).ReturnsAsync(false);
            var command = new CreateAccountGroupCommand { GroupCode = "A", GroupName = "Assets", AccountTypeId = 3, ParentAccountGroupId = null, SortOrder = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_Level1MissingAccountType_Fails()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            var command = new CreateAccountGroupCommand { GroupCode = "A", GroupName = "Assets", AccountTypeId = null, ParentAccountGroupId = null, SortOrder = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AccountTypeId);
        }

        [Fact]
        public async Task Validate_Level1NonExistentAccountType_Fails()
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null)).ReturnsAsync(false);
            _mockAccountTypeRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = new CreateAccountGroupCommand { GroupCode = "A", GroupName = "Assets", AccountTypeId = 99, ParentAccountGroupId = null, SortOrder = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AccountTypeId);
        }
    }
}
