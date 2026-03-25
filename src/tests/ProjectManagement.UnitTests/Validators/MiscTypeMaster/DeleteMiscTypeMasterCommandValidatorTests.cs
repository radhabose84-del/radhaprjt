using FluentValidation.TestHelper;
using ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster;
using ProjectManagement.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster;
using ProjectManagement.Presentation.Validation.MiscTypeMaster;

namespace ProjectManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class DeleteMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 1 });

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            // All rule chains run independently in FluentValidation (separate RuleFor calls),
            // so async rules are still invoked even when NotEmpty fails for Id = 0.
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(0)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 0 });

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_HasChildRecords_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(5)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(5)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 5 });

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.SoftDeleteValidation(99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteMiscTypeMasterCommand { Id = 99 });

            result.ShouldHaveAnyValidationError();
        }
    }
}
