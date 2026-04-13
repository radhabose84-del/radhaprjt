using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MaintenanceManagement.Application.MaintenanceRequest.Command.CreateMaintenanceRequest;
using MaintenanceManagement.Presentation.Validation.MaintenanceRequest;

namespace MaintenanceManagement.UnitTests.Validators.MaintenanceRequest
{
    public sealed class CreateMaintenanceRequestCommandValidatorBatchATests
    {
        private readonly Mock<IMaintenanceRequestQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateMaintenanceRequestCommandValidator CreateValidator() =>
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
    }
}
