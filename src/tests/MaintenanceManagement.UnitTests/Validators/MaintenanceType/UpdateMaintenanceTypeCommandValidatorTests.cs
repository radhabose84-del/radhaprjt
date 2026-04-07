using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.MaintenanceType;
using MaintenanceManagement.UnitTests.TestData;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceType
{
    public sealed class UpdateMaintenanceTypeCommandValidatorTests
    {
        private readonly Mock<IMaintenanceTypeCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMaintenanceTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateMaintenanceTypeCommandValidator CreateValidator() =>
            new(_mockCommandRepo.Object, _mockMaxLength.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1, string typeName = "Corrective")
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(MaintenanceTypeBuilders.ValidEntity(id));
            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(typeName, id))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MaintenanceTypeBuilders.ValidUpdateCommand();
            SetupHappyPath(command.Id, command.TypeName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTypeName_FailsValidation(string? typeName)
        {
            var command = MaintenanceTypeBuilders.ValidUpdateCommand(typeName: typeName!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            var command = MaintenanceTypeBuilders.ValidUpdateCommand(id: 999);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((MaintenanceManagement.Domain.Entities.MaintenanceType?)null);
            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(command.TypeName, 999))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
