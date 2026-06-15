using FluentValidation.TestHelper;
using FinanceManagement.Application.AccountGroup.Commands.MoveAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Presentation.Validation.AccountGroup;

namespace FinanceManagement.UnitTests.Validators.AccountGroup
{
    public sealed class MoveAccountGroupCommandValidatorTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private MoveAccountGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        // Move a level-3 node (Id 10) under a level-2 parent (Id 5).
        private static MoveAccountGroupCommand ValidCommand() =>
            new() { Id = 10, NewParentAccountGroupId = 5, Justification = "Restructure for FY2026 reporting", ApproverId = 99 };

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(10)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ParentExistsAsync(5)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsDescendantAsync(10, 5)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetLevelAsync(10)).ReturnsAsync(3);
            _mockQueryRepo.Setup(r => r.GetLevelAsync(5)).ReturnsAsync(2);
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NotFound_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(10)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_SelfMove_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.ParentExistsAsync(10)).ReturnsAsync(true);
            var command = ValidCommand();
            command.NewParentAccountGroupId = command.Id; // 10
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.NewParentAccountGroupId);
        }

        [Fact]
        public async Task Validate_ParentDoesNotExist_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.ParentExistsAsync(5)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.NewParentAccountGroupId);
        }

        [Fact]
        public async Task Validate_CircularMove_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.IsDescendantAsync(10, 5)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.NewParentAccountGroupId);
        }

        [Fact]
        public async Task Validate_WrongParentLevel_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.GetLevelAsync(5)).ReturnsAsync(1); // not nodeLevel - 1
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.NewParentAccountGroupId);
        }

        [Fact]
        public async Task Validate_ShortJustification_Fails()
        {
            SetupHappyPath();
            var command = ValidCommand();
            command.Justification = "too short";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Justification);
        }

        [Fact]
        public async Task Validate_ZeroApprover_Fails()
        {
            SetupHappyPath();
            var command = ValidCommand();
            command.ApproverId = 0;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ApproverId);
        }
    }
}
