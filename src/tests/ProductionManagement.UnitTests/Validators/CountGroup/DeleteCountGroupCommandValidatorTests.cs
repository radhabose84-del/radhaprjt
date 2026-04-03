using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.ICountGroup;
using ProductionManagement.Application.CountGroup.Commands.DeleteCountGroup;
using ProductionManagement.Presentation.Validation.CountGroup;

namespace ProductionManagement.UnitTests.Validators.CountGroup
{
    public sealed class DeleteCountGroupCommandValidatorTests
    {
        private readonly Mock<ICountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteCountGroupCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteCountGroupCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteCountGroupCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteCountGroupCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
