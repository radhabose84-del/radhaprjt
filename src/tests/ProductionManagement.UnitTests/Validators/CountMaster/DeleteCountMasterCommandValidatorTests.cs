using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Commands.DeleteCountMaster;
using ProductionManagement.Presentation.Validation.CountMaster;

namespace ProductionManagement.UnitTests.Validators.CountMaster
{
    public sealed class DeleteCountMasterCommandValidatorTests
    {
        private readonly Mock<ICountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteCountMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteCountMasterCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteCountMasterCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteCountMasterCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
