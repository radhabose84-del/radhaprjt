using FluentValidation.TestHelper;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.CreateWarehouseMaster;
using WarehouseManagement.Presentation.Validation.WarehouseMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Validators.WarehouseMaster
{
    public sealed class CreateWarehouseMasterCommandValidatorTests
    {
        private readonly Mock<IWarehouseMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateWarehouseMasterCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string name = "Test Warehouse")
        {
            _mockQueryRepo.Setup(r => r.ExistsByNameAsync(name, null)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.WarehouseName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand(name: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WarehouseName);
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand(name: "Existing");
            _mockQueryRepo.Setup(r => r.ExistsByNameAsync("Existing", null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WarehouseName)
                  .WithErrorMessage("Warehouse name 'Existing' already exists.");
        }

        [Fact]
        public async Task Validate_ZeroUnitId_FailsValidation()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand();
            command.UnitId = 0;
            SetupAllAsyncMocks(command.WarehouseName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }

        [Fact]
        public async Task Validate_InvalidPincode_FailsValidation()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand();
            command.Pincode = "123"; // not 6 digits
            SetupAllAsyncMocks(command.WarehouseName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Pincode);
        }

        [Fact]
        public async Task Validate_ZeroMaxCapacity_FailsValidation()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand(maxCapacity: 0);
            SetupAllAsyncMocks(command.WarehouseName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MaxCapacity);
        }

        [Fact]
        public async Task Validate_ZeroWarehouseTypeId_FailsValidation()
        {
            var command = WarehouseMasterBuilders.ValidCreateCommand(warehouseTypeId: 0);
            SetupAllAsyncMocks(command.WarehouseName);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WarehouseTypeId);
        }
    }
}
