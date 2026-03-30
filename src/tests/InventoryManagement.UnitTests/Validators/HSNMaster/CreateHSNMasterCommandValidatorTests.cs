using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Presentation.Validation.HSNMaster;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.HSNMaster
{
    public sealed class CreateHSNMasterCommandValidatorTests
    {
        private readonly Mock<IHSNMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateHSNMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(string hsnCode = "1001")
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(hsnCode, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = HSNMasterBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.HSNCode);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyHSNCode_FailsValidation(string? hsnCode)
        {
            var command = HSNMasterBuilders.ValidCreateCommand(hsnCode: hsnCode!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateHSNCode_FailsValidation()
        {
            var command = HSNMasterBuilders.ValidCreateCommand(hsnCode: "EXIST01");
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("EXIST01", null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.HSNCode);
        }

        [Fact]
        public async Task Validate_ZeroTypeId_FailsValidation()
        {
            var command = HSNMasterBuilders.ValidCreateCommand();
            command = new InventoryManagement.Application.HSNMaster.Command.CreateHSNMaster.CreateHSNMasterCommand
            {
                HSNCode = command.HSNCode,
                Description = command.Description,
                TypeId = 0,
                GSTCategoryId = command.GSTCategoryId,
                GSTPercentage = command.GSTPercentage,
                IGSTPercentage = command.IGSTPercentage,
                ValidFrom = command.ValidFrom
            };
            SetupAllAsyncMocks(command.HSNCode);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TypeId);
        }
    }
}
