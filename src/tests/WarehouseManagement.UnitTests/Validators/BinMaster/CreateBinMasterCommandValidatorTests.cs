using FluentValidation.TestHelper;
using WarehouseManagement.Application.BinMaster.Command.CreateBinMaster;
using WarehouseManagement.Application.Common.Interfaces.IBinMaster;
using WarehouseManagement.Presentation.Validation.BinMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Validators.BinMaster
{
    public sealed class CreateBinMasterCommandValidatorTests
    {
        private readonly Mock<IBinMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateBinMasterCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = BinMasterBuilders.ValidCreateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroWarehouseId_FailsValidation()
        {
            var command = BinMasterBuilders.ValidCreateCommand(warehouseId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WarehouseId);
        }

        [Fact]
        public async Task Validate_ZeroBinCapacity_FailsValidation()
        {
            var command = BinMasterBuilders.ValidCreateCommand(binCapacity: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BinCapacity);
        }

        [Fact]
        public async Task Validate_ZeroCapacityUOMId_FailsValidation()
        {
            var command = BinMasterBuilders.ValidCreateCommand(capacityUOMId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CapacityUOMId);
        }

        [Fact]
        public async Task Validate_LongBinName_FailsValidation()
        {
            var command = BinMasterBuilders.ValidCreateCommand();
            command.BinName = new string('A', 51);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BinName);
        }

        [Fact]
        public async Task Validate_ZeroRackId_FailsValidation()
        {
            var command = BinMasterBuilders.ValidCreateCommand();
            command.RackId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RackId);
        }
    }
}
