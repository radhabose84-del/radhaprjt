using FluentValidation.TestHelper;
using FinanceManagement.Application.AccountGroup.Commands.DeleteAccountGroup;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Presentation.Validation.AccountGroup;

namespace FinanceManagement.UnitTests.Validators.AccountGroup
{
    public sealed class DeleteAccountGroupCommandValidatorTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteAccountGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.HasChildrenAsync(1)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidId_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(new DeleteAccountGroupCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_Fails()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteAccountGroupCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteAccountGroupCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_HasChildren_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.HasChildrenAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteAccountGroupCommand(1));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
