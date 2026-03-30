using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceType;
using MaintenanceManagement.Presentation.Validation.MaintenanceType;
using MaintenanceManagement.UnitTests.TestData;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceType
{
    public sealed class DeleteMaintenanceTypeCommandValidatorTests
    {
        private readonly Mock<IMaintenanceTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteMaintenanceTypeCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MaintenanceTypeBuilders.ValidDeleteCommand(1);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(MaintenanceTypeBuilders.ValidEntity(1));

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = MaintenanceTypeBuilders.ValidDeleteCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            var command = MaintenanceTypeBuilders.ValidDeleteCommand(999);
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((MaintenanceManagement.Domain.Entities.MaintenanceType?)null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
