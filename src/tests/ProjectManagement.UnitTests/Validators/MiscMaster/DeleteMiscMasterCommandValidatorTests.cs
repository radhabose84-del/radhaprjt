using FluentValidation.TestHelper;
using ProjectManagement.Application.Common.Interfaces.IMiscMaster;
using ProjectManagement.Application.MiscMaster.Command.DeleteMiscMaster;
using ProjectManagement.Presentation.Validation.MiscMaster;

namespace ProjectManagement.UnitTests.Validators.MiscMaster
{
    public sealed class DeleteMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            // All rule chains run independently in FluentValidation (separate RuleFor calls),
            // so NotFoundAsync is still invoked even when NotEmpty fails for Id = 0.
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscMasterCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
