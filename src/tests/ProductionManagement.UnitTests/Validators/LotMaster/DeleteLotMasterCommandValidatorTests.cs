using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.ILotMaster;
using ProductionManagement.Application.LotMaster.Commands.DeleteLotMaster;
using ProductionManagement.Presentation.Validation.LotMaster;

namespace ProductionManagement.UnitTests.Validators.LotMaster
{
    public sealed class DeleteLotMasterCommandValidatorTests
    {
        private readonly Mock<ILotMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteLotMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteLotMasterCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteLotMasterCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteLotMasterCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
