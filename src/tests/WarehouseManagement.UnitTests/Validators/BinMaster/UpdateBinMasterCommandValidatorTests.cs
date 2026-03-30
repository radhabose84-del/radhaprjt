using FluentValidation.TestHelper;
using WarehouseManagement.Application.BinMaster.Command.UpdateBinMaster;
using WarehouseManagement.Presentation.Validation.BinMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Validators.BinMaster
{
    public sealed class UpdateBinMasterCommandValidatorTests
    {
        private UpdateBinMasterCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = BinMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = BinMasterBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ZeroBinCapacity_FailsValidation()
        {
            var command = BinMasterBuilders.ValidUpdateCommand();
            command.BinCapacity = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BinCapacity);
        }

        [Fact]
        public async Task Validate_ZeroCapacityUOMId_FailsValidation()
        {
            var command = BinMasterBuilders.ValidUpdateCommand();
            command.CapacityUOMId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CapacityUOMId);
        }

        [Fact]
        public async Task Validate_LongBinName_FailsValidation()
        {
            var command = BinMasterBuilders.ValidUpdateCommand();
            command.BinName = new string('A', 51);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.BinName);
        }
    }
}
