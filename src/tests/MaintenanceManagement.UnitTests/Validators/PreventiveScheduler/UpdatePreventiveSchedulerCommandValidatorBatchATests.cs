using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler;
using MaintenanceManagement.Presentation.Validation.PreventiveSchedulers;

namespace MaintenanceManagement.UnitTests.Validators.PreventiveScheduler
{
    public sealed class UpdatePreventiveSchedulerCommandValidatorBatchATests
    {
        private readonly Mock<IMachineGroupQueryRepository> _mockMachineGroupRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterRepo = new(MockBehavior.Loose);
        private readonly Mock<IActivityMasterQueryRepository> _mockActivityRepo = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerQuery> _mockPreventiveQuery = new(MockBehavior.Loose);

        private UpdatePreventiveSchedulerCommandValidator CreateValidator() =>
            new(_mockMachineGroupRepo.Object, _mockMiscMasterRepo.Object,
                _mockActivityRepo.Object, _mockPreventiveQuery.Object);

        [Fact]
        public void Constructor_WithValidQueries_DoesNotThrow()
        {
            var act = () => CreateValidator();
            act.Should().NotThrow();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new UpdatePreventiveSchedulerCommand
            {
                Id = 0,
                Activity = new()
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyActivityList_FailsValidation()
        {
            var command = new UpdatePreventiveSchedulerCommand
            {
                Id = 1,
                MachineGroupId = 1,
                DepartmentId = 1,
                Activity = new()
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_HasErrorForDepartmentId()
        {
            var command = new UpdatePreventiveSchedulerCommand
            {
                Id = 1,
                MachineGroupId = 1,
                DepartmentId = 0,
                Activity = new()
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
        }
    }
}
