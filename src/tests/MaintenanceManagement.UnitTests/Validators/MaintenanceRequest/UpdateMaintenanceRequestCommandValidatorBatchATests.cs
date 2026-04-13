using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestCommand;
using MaintenanceManagement.Presentation.Validation.MaintenanceRequest;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceRequest
{
    public sealed class UpdateMaintenanceRequestCommandValidatorBatchATests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateMaintenanceRequestCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public void Constructor_WithValidRepo_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_ZeroMachineId_FailsValidation()
        {
            var command = new UpdateMaintenanceRequestCommand
            {
                Id = 1,
                RequestTypeId = 1,
                MaintenanceTypeId = 1,
                MachineId = 0
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroMaintenanceTypeId_HasError()
        {
            var command = new UpdateMaintenanceRequestCommand
            {
                Id = 1,
                RequestTypeId = 1,
                MaintenanceTypeId = 0,
                MachineId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MaintenanceTypeId);
        }

        [Fact]
        public async Task Validate_ZeroRequestTypeId_FailsValidation()
        {
            var command = new UpdateMaintenanceRequestCommand
            {
                Id = 1,
                RequestTypeId = 0,
                MaintenanceTypeId = 1,
                MachineId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
