using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IYarnTwistMaster;
using ProductionManagement.Application.YarnTwistMaster.Commands.DeleteYarnTwistMaster;
using ProductionManagement.Presentation.Validation.YarnTwistMaster;

namespace ProductionManagement.UnitTests.Validators.YarnTwistMaster
{
    public sealed class DeleteYarnTwistMasterCommandValidatorTests
    {
        private readonly Mock<IYarnTwistMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteYarnTwistMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteYarnTwistMasterCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteYarnTwistMasterCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteYarnTwistMasterCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
