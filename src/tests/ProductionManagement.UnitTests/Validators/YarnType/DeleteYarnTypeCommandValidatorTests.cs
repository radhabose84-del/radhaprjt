using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Application.YarnType.Commands.DeleteYarnType;
using ProductionManagement.Presentation.Validation.YarnType;

namespace ProductionManagement.UnitTests.Validators.YarnType
{
    public sealed class DeleteYarnTypeCommandValidatorTests
    {
        private readonly Mock<IYarnTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteYarnTypeCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteYarnTypeCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteYarnTypeCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteYarnTypeCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
