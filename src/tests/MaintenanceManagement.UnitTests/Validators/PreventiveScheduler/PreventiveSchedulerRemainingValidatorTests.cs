using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MachineWiseFrequencyUpdate;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.MapMachine;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.ActiveInActivePreventive;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.UpdatePreventiveScheduler;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.PreventiveSchedulers;

namespace MaintenanceManagement.UnitTests.Validators.PreventiveScheduler
{
    public sealed class UpdatePreventiveSchedulerCommandValidatorTests
    {
        private readonly Mock<IMachineGroupQueryRepository> _mockMachineGroupRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterRepo = new(MockBehavior.Loose);
        private readonly Mock<IActivityMasterQueryRepository> _mockActivityRepo = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerQuery> _mockPreventiveQuery = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_ZeroMachineGroupId_FailsValidation()
        {
            var validator = new UpdatePreventiveSchedulerCommandValidator(
                _mockMachineGroupRepo.Object, _mockMiscMasterRepo.Object,
                _mockActivityRepo.Object, _mockPreventiveQuery.Object);
            var command = new UpdatePreventiveSchedulerCommand { Id = 0, MachineGroupId = 0, Activity = new() };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class UpdateActiveStatusCommandValidatorTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var validator = new UpdateActiveStatusCommandValidator(_mockQuery.Object);
            var command = new ActiveInActivePreventiveCommand { Id = 0 };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class MapMachineCommandValidatorTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);
        private readonly Mock<IMachineMasterQueryRepository> _mockMachineRepo = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_ZeroPreventiveSchedulerId_FailsValidation()
        {
            var validator = new MapMachineCommandValidator(_mockQuery.Object, _mockMachineRepo.Object);
            var command = new MapMachineCommand { Id = 0 };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class MachineWiseFrequencyUpdateCommandValidatorTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_ZeroPreventiveSchedulerId_FailsValidation()
        {
            var validator = new MachineWiseFrequencyUpdateCommandValidator(_mockQuery.Object);
            var command = new MachineWiseFrequencyUpdateCommand { Id = 0 };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }

    public sealed class BulkImportPreventiveSchedulerCommandValidatorTests
    {
        private readonly Mock<IPreventiveSchedulerQuery> _mockQuery = new(MockBehavior.Loose);

        [Fact]
        public async Task Validate_NullFile_FailsValidation()
        {
            var validator = new BulkImportPreventiveSchedulerCommandValidator(_mockQuery.Object);
            var command = new RescheduleBulkImportCommand { File = null };
            var result = await validator.TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
