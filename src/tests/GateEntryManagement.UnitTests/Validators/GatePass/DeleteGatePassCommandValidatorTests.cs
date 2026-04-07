using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Commands.DeleteGatePass;
using GateEntryManagement.Presentation.Validation.GatePass;

namespace GateEntryManagement.UnitTests.Validators.GatePass
{
    public sealed class DeleteGatePassCommandValidatorTests
    {
        private readonly Mock<IGatePassQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteGatePassCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteGatePassCommand(1);
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteGatePassCommand(0);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new DeleteGatePassCommand(999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
