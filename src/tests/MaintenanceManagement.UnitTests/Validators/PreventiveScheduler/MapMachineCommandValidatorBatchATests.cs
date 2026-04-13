using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine;
using MaintenanceManagement.Presentation.Validation.PreventiveSchedulers;

namespace MaintenanceManagement.UnitTests.Validators.PreventiveScheduler
{
    public sealed class MapMachineCommandValidatorBatchATests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IMachineMasterQueryRepository> _mockMachineRepo = new(MockBehavior.Loose);

        private MapMachineCommandValidator CreateValidator() =>
            new(_mockQuery.Object, _mockMachineRepo.Object);

        [Fact]
        public void Constructor_WithValidQueries_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new MapMachineCommand { Id = 0, MachineId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroMachineId_HasErrorForMachineId()
        {
            var command = new MapMachineCommand { Id = 1, MachineId = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MachineId);
        }

        [Fact]
        public async Task Validate_DefaultLastMaintenanceDate_FailsValidation()
        {
            var command = new MapMachineCommand
            {
                Id = 1,
                MachineId = 1,
                LastMaintenanceActivityDate = DateOnly.MinValue
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
