using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.DeleteUserSignature;
using UserManagement.Presentation.Validation.UserSignature;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.UserSignature
{
    public sealed class DeleteUserSignatureCommandValidatorTests
    {
        private readonly Mock<IUserSignatureQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteUserSignatureCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand(id: 1);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            var command = UserSignatureBuilders.ValidDeleteCommand(id: 999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
