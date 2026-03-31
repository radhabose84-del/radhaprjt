using FluentValidation.TestHelper;
using WarehouseManagement.Application.Common.Interfaces.IRackMaster;
using WarehouseManagement.Application.RackMaster.Command.CreateRackMaster;
using WarehouseManagement.Presentation.Validation.RackMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Validators.RackMaster
{
    public sealed class CreateRackMasterCommandValidatorTests
    {
        private readonly Mock<IRackMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateRackMasterCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

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
            var command = RackMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroWarehouseId_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = RackMasterBuilders.ValidCreateCommand(warehouseId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WarehouseId);
        }

        [Fact]
        public async Task Validate_DuplicateSlot_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.RackSlotAlreadyExistsAsync(1, 1, 1, 1, null)).ReturnsAsync(true);
            var command = RackMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NegativeMaxCapacity_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = RackMasterBuilders.ValidCreateCommand();
            command.MaxCapacity = -1;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MaxCapacity);
        }

        [Fact]
        public async Task Validate_LongRackName_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = RackMasterBuilders.ValidCreateCommand(rackName: new string('A', 101));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RackName);
        }
    }
}
