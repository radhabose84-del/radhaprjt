using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class PreventiveSchedulerDetailEntityTests
    {
        [Fact]
        public void PreventiveSchedulerDetail_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PreventiveSchedulerDetail
            {
                PreventiveScheduler = new PreventiveSchedulerHeader
                {
                    MachineGroup = new MachineGroup(),
                    MiscMaintenanceCategory = new MiscMaster(),
                    MiscSchedule = new MiscMaster(),
                    MiscFrequencyType = new MiscMaster(),
                    MiscFrequencyUnit = new MiscMaster()
                },
                Machine = new MachineMaster()
            };
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PreventiveSchedulerDetail_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PreventiveSchedulerDetail)).Should().BeTrue();
        }

        [Fact]
        public void PreventiveSchedulerDetail_Properties_ShouldBeAssignable()
        {
            var entity = new PreventiveSchedulerDetail
            {
                Id = 1,
                PreventiveSchedulerHeaderId = 2,
                PreventiveScheduler = new PreventiveSchedulerHeader
                {
                    MachineGroup = new MachineGroup(),
                    MiscMaintenanceCategory = new MiscMaster(),
                    MiscSchedule = new MiscMaster(),
                    MiscFrequencyType = new MiscMaster(),
                    MiscFrequencyUnit = new MiscMaster()
                },
                MachineId = 3,
                Machine = new MachineMaster(),
                WorkOrderCreationStartDate = new DateOnly(2025, 1, 15),
                FrequencyInterval = 30,
                GraceDays = 3,
                HangfireJobId = "job-123"
            };
            entity.Id.Should().Be(1);
            entity.PreventiveSchedulerHeaderId.Should().Be(2);
            entity.MachineId.Should().Be(3);
            entity.HangfireJobId.Should().Be("job-123");
        }

        [Fact]
        public void PreventiveSchedulerDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PreventiveSchedulerDetail
            {
                PreventiveScheduler = new PreventiveSchedulerHeader
                {
                    MachineGroup = new MachineGroup(),
                    MiscMaintenanceCategory = new MiscMaster(),
                    MiscSchedule = new MiscMaster(),
                    MiscFrequencyType = new MiscMaster(),
                    MiscFrequencyUnit = new MiscMaster()
                },
                Machine = new MachineMaster(),
                ActualWorkOrderDate = null,
                RescheduleReason = null,
                HangfireJobId = null,
                LastMaintenanceActivityDate = null,
                ScheduleId = null,
                FrequencyTypeId = null,
                FrequencyUnitId = null
            };
            entity.ActualWorkOrderDate.Should().BeNull();
            entity.HangfireJobId.Should().BeNull();
            entity.ScheduleId.Should().BeNull();
        }
    }
}
