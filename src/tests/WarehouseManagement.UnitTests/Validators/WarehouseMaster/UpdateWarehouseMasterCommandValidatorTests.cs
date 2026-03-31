using FluentValidation.TestHelper;
using WarehouseManagement.Application.Common.Interfaces.IWarehouseMaster;
using WarehouseManagement.Application.WarehouseMaster.Command.UpdateWarehouseMaster;
using WarehouseManagement.Presentation.Validation.WarehouseMaster;
using WarehouseManagement.UnitTests.TestData;

namespace WarehouseManagement.UnitTests.Validators.WarehouseMaster
{
    public sealed class UpdateWarehouseMasterCommandValidatorTests
    {
        private readonly Mock<IWarehouseMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateWarehouseMasterCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string? name = "Updated Warehouse", int id = 1)
        {
            _mockQueryRepo.Setup(r => r.ExistsByNameAsync(name!, id)).ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = WarehouseMasterBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command.WarehouseName, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyId_FailsValidation()
        {
            var command = WarehouseMasterBuilders.ValidUpdateCommand(id: 0);
            SetupAllAsyncMocks(command.WarehouseName, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyName_FailsValidation(string? name)
        {
            var command = WarehouseMasterBuilders.ValidUpdateCommand(name: name);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WarehouseName);
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            var command = WarehouseMasterBuilders.ValidUpdateCommand(name: "Existing");
            _mockQueryRepo.Setup(r => r.ExistsByNameAsync("Existing", command.Id)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.WarehouseName)
                  .WithErrorMessage("Warehouse name 'Existing' already exists.");
        }

        [Fact]
        public async Task Validate_NullItemGroupIds_FailsValidation()
        {
            var command = WarehouseMasterBuilders.ValidUpdateCommand();
            command.AllowedItemGroupIds = null;
            SetupAllAsyncMocks(command.WarehouseName, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AllowedItemGroupIds);
        }

        [Fact]
        public async Task Validate_EmptyItemGroupIds_FailsValidation()
        {
            var command = WarehouseMasterBuilders.ValidUpdateCommand();
            command.AllowedItemGroupIds = new List<int>();
            SetupAllAsyncMocks(command.WarehouseName, command.Id);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AllowedItemGroupIds);
        }
    }
}
