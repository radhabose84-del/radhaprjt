using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader;
using ProductionManagement.Application.YarnConversionHeader.Commands.DeleteYarnConversionHeader;
using ProductionManagement.Presentation.Validation.YarnConversionHeader;

namespace ProductionManagement.UnitTests.Validators.YarnConversionHeader
{
    public sealed class DeleteYarnConversionHeaderCommandValidatorTests
    {
        private readonly Mock<IYarnConversionHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteYarnConversionHeaderCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteYarnConversionHeaderCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteYarnConversionHeaderCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteYarnConversionHeaderCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
