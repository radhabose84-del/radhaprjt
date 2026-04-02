using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IProcessMaster;
using ProductionManagement.Application.ProcessMaster.Commands.DeleteProcessMaster;
using ProductionManagement.Presentation.Validation.ProcessMaster;

namespace ProductionManagement.UnitTests.Validators.ProcessMaster
{
    public sealed class DeleteProcessMasterCommandValidatorTests
    {
        private readonly Mock<IProcessMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteProcessMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteProcessMasterCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteProcessMasterCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteProcessMasterCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
