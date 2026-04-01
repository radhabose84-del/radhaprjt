using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IPackType;
using ProductionManagement.Application.PackType.Commands.DeletePackType;
using ProductionManagement.Presentation.Validation.PackType;

namespace ProductionManagement.UnitTests.Validators.PackType
{
    public sealed class DeletePackTypeCommandValidatorTests
    {
        private readonly Mock<IPackTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeletePackTypeCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeletePackTypeCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeletePackTypeCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeletePackTypeCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
