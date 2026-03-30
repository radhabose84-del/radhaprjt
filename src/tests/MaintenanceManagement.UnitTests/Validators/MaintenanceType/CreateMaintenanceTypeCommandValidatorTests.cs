using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MaintenanceType;
using MaintenanceManagement.UnitTests.TestData;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceType
{
    public sealed class CreateMaintenanceTypeCommandValidatorTests
    {
        private readonly Mock<IMaintenanceTypeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private CreateMaintenanceTypeCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockMaxLength.Object);

        private void SetupAllAsyncMocks(string typeName = "Preventive")
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(typeName))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MaintenanceTypeBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.TypeName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTypeName_FailsValidation(string? typeName)
        {
            var command = MaintenanceTypeBuilders.ValidCreateCommand(typeName: typeName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateTypeName_FailsValidation()
        {
            var command = MaintenanceTypeBuilders.ValidCreateCommand(typeName: "Preventive");
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync("Preventive"))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
