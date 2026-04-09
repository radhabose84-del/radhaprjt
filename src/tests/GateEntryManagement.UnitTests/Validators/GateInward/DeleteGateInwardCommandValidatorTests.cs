using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Commands.DeleteGateInward;
using GateEntryManagement.Presentation.Validation.GateInward;

namespace GateEntryManagement.UnitTests.Validators.GateInward
{
    public sealed class DeleteGateInwardCommandValidatorTests
    {
        private readonly Mock<IGateInwardQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteGateInwardCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteGateInwardCommand(1);
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteGateInwardCommand(0);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new DeleteGateInwardCommand(999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
