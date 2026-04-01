using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Commands.DeleteRepacking;
using ProductionManagement.Presentation.Validation.Repacking;

namespace ProductionManagement.UnitTests.Validators.Repacking
{
    public sealed class DeleteRepackingCommandValidatorTests
    {
        private readonly Mock<IRepackingQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteRepackingCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteRepackingCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteRepackingCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteRepackingCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
