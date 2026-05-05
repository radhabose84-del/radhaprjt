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

        // SCRUM-1475: duplicate-machine guard on Update flow

        [Fact]
        public async Task Validate_ReassigningToBusyMachine_FailsValidation()
        {
            // Updating request 10 to point at machine 5, but machine 5 already has a
            // *different* active request → validator should block.
            _mockQueryRepo
                .Setup(r => r.HasActiveRequestForMachineAsync(5, 10))
                .ReturnsAsync(true);

            var command = new UpdateMaintenanceRequestCommand
            {
                Id = 10,
                RequestTypeId = 1,
                MaintenanceTypeId = 1,
                MachineId = 5
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MachineId)
                  .WithErrorMessage("A request for this machine is already Open / In Progress. " +
                                    "Please resolve the existing request before reassigning to it.");
        }

        [Fact]
        public async Task Validate_SelfUpdate_DoesNotTriggerDuplicateGuard()
        {
            // Updating request 10 without changing the machine. The only active row for
            // machine 5 is the request being updated — repo returns false because we
            // pass excludeRequestId = 10, so the validator should NOT raise the dup error.
            _mockQueryRepo
                .Setup(r => r.HasActiveRequestForMachineAsync(5, 10))
                .ReturnsAsync(false);

            var command = new UpdateMaintenanceRequestCommand
            {
                Id = 10,
                RequestTypeId = 1,
                MaintenanceTypeId = 1,
                MachineId = 5
            };

            var result = await CreateValidator().TestValidateAsync(command);

            // No duplicate-machine error
            result.Errors.Should().NotContain(e =>
                e.PropertyName == nameof(UpdateMaintenanceRequestCommand.MachineId) &&
                e.ErrorMessage.StartsWith("A request for this machine is already"));
        }
    }
}
