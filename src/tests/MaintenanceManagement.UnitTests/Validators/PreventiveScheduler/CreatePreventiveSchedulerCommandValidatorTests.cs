using FluentValidation.TestHelper;
using MaintenanceManagement.Application.Common.Interfaces.IActivityMaster;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.Common.Interfaces.IMiscMaster;
using MaintenanceManagement.Application.Common.Interfaces.IPreventiveScheduler;
using MaintenanceManagement.Application.PreventiveSchedulers.Commands.CreatePreventiveScheduler;
using MaintenanceManagement.Presentation.Validation.Common;
using MaintenanceManagement.Presentation.Validation.PreventiveSchedulers;

namespace MaintenanceManagement.UnitTests.Validators.PreventiveScheduler
{
    public sealed class CreatePreventiveSchedulerCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IMachineGroupQueryRepository> _mockMachineGroupRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscMasterRepo = new(MockBehavior.Loose);
        private readonly Mock<IActivityMasterQueryRepository> _mockActivityRepo = new(MockBehavior.Loose);
        private readonly Mock<IPreventiveSchedulerQuery> _mockPreventiveQuery = new(MockBehavior.Loose);

        private CreatePreventiveSchedulerCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockMachineGroupRepo.Object, _mockMiscMasterRepo.Object,
                _mockActivityRepo.Object, _mockPreventiveQuery.Object);

        private static CreatePreventiveSchedulerCommand ValidCommand() => new()
        {
            MachineGroupId = 1,
            DepartmentId = 1,
            MaintenanceCategoryId = 1,
            ScheduleId = 1,
            FrequencyTypeId = 1,
            FrequencyInterval = 30,
            FrequencyUnitId = 1,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            GraceDays = 3,
            ReminderWorkOrderDays = 5,
            ReminderMaterialReqDays = 7,
            IsDownTimeRequired = 1,
            DownTimeEstimateHrs = 4,
            Activity = new List<PreventiveSchedulerActivityDto> { new() { ActivityId = 1 } }
        };

        private void SetupAllFKMocks()
        {
            _mockMachineGroupRepo.Setup(r => r.FKColumnExistValidation(It.IsAny<int>())).ReturnsAsync(true);
            _mockMiscMasterRepo.Setup(r => r.FKColumnValidation(It.IsAny<int>())).ReturnsAsync(true);
            _mockActivityRepo.Setup(r => r.FKColumnExistValidation(It.IsAny<int>())).ReturnsAsync(true);
            _mockPreventiveQuery.Setup(r => r.AlreadyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);
            _mockPreventiveQuery.Setup(r => r.MachingroupValidation(It.IsAny<int>())).ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllFKMocks();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroMachineGroupId_FailsValidation()
        {
            var command = ValidCommand();
            command.MachineGroupId = 0;

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_EmptyActivity_FailsValidation()
        {
            var command = ValidCommand();
            command.Activity = new List<PreventiveSchedulerActivityDto>();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ReminderGreaterThanFrequency_FailsValidation()
        {
            SetupAllFKMocks();
            var command = ValidCommand();
            command.ReminderWorkOrderDays = 50; // > FrequencyInterval (30)

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
