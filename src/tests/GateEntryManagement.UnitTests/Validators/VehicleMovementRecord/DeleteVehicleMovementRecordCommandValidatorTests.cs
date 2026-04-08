using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.DeleteVehicleMovementRecord;
using GateEntryManagement.Presentation.Validation.VehicleMovementRecord;

namespace GateEntryManagement.UnitTests.Validators.VehicleMovementRecord
{
    public sealed class DeleteVehicleMovementRecordCommandValidatorTests
    {
        private readonly Mock<IVehicleMovementRecordQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteVehicleMovementRecordCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteVehicleMovementRecordCommand(1);
            SetupAllAsyncMocks(1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteVehicleMovementRecordCommand(0);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new DeleteVehicleMovementRecordCommand(999);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
