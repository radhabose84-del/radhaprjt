using FluentValidation.TestHelper;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.UpdateRackMaster;
using WarehouseManagement.Presentation.Validation.RackMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Validators.RackMaster
{
    public sealed class UpdateRackMasterCommandValidatorTests
    {
        private readonly Mock<IRackMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateRackMasterCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks()
        {
            _mockQueryRepo.Setup(r => r.RackSlotAlreadyExistsAsync(
                It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = RackMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = RackMasterBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroWarehouseId_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = RackMasterBuilders.ValidUpdateCommand(warehouseId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WarehouseId);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = RackMasterBuilders.ValidUpdateCommand(isActive: 2);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_PartialSlotFields_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = RackMasterBuilders.ValidUpdateCommand();
            command.FloorId = 1;
            command.AisleId = 1;
            command.RackLevelId = null; // partial

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
