using Contracts.Interfaces.Lookups.Users;
using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MaintenanceManagement.Presentation.Validation.MaintenanceRequest;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceRequest
{
    public sealed class CreateMaintenanceRequestCommandValidatorBatchATests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);

        private CreateMaintenanceRequestCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockDeptLookup.Object);

        [Fact]
        public void Constructor_WithValidRepo_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_ZeroMachineId_FailsValidation()
        {
            var command = new CreateMaintenanceRequestCommand
            {
                RequestTypeId = 1,
                MaintenanceTypeId = 1,
                MachineId = 0
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroRequestTypeId_HasErrorForRequestType()
        {
            var command = new CreateMaintenanceRequestCommand
            {
                RequestTypeId = 0,
                MaintenanceTypeId = 1,
                MachineId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.RequestTypeId);
        }

        [Fact]
        public async Task Validate_EmptyMaintenanceTypeId_FailsValidation()
        {
            var command = new CreateMaintenanceRequestCommand
            {
                RequestTypeId = 1,
                MaintenanceTypeId = 0,
                MachineId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_AllValid_PassesBasicChecks()
        {
            // FK existence rule needs a positive mock; otherwise loose mock returns default(false)
            // and the MachineId rule legitimately fails.
            _mockQueryRepo.Setup(r => r.MachineExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var command = new CreateMaintenanceRequestCommand
            {
                RequestTypeId = 1,
                MaintenanceTypeId = 1,
                MachineId = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            // Validator may still have other errors but not for the checked fields
            result.ShouldNotHaveValidationErrorFor(x => x.MachineId);
            result.ShouldNotHaveValidationErrorFor(x => x.RequestTypeId);
        }

        // SCRUM-1475: duplicate-machine guard

        [Fact]
        public async Task Validate_MachineHasActiveRequest_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.HasActiveRequestForMachineAsync(5, null))
                .ReturnsAsync(true);

            var command = new CreateMaintenanceRequestCommand
            {
                RequestTypeId = 1,
                MaintenanceTypeId = 1,
                MachineId = 5
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MachineId)
                  .WithErrorMessage("A request for this machine is already Open / In Progress. " +
                                    "Please resolve the existing request before creating a new one.");
        }

        [Fact]
        public async Task Validate_MachineHasNoActiveRequest_PassesDuplicateCheck()
        {
            _mockQueryRepo
                .Setup(r => r.HasActiveRequestForMachineAsync(5, null))
                .ReturnsAsync(false);

            var command = new CreateMaintenanceRequestCommand
            {
                RequestTypeId = 1,
                MaintenanceTypeId = 1,
                MachineId = 5
            };

            var result = await CreateValidator().TestValidateAsync(command);

            // No error specifically for the duplicate-machine rule
            result.Errors.Should().NotContain(e =>
                e.PropertyName == nameof(CreateMaintenanceRequestCommand.MachineId) &&
                e.ErrorMessage.StartsWith("A request for this machine is already"));
        }
    }
}
