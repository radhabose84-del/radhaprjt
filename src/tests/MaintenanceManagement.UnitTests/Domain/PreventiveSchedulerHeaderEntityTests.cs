using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class PreventiveSchedulerHeaderEntityTests
    {
        [Fact]
        public void PreventiveSchedulerHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new PreventiveSchedulerHeader
            {
                MachineGroup = new MachineGroup(),
                MiscMaintenanceCategory = new MiscMaster(),
                MiscSchedule = new MiscMaster(),
                MiscFrequencyType = new MiscMaster(),
                MiscFrequencyUnit = new MiscMaster()
            };
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void PreventiveSchedulerHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new PreventiveSchedulerHeader
            {
                MachineGroup = new MachineGroup(),
                MiscMaintenanceCategory = new MiscMaster(),
                MiscSchedule = new MiscMaster(),
                MiscFrequencyType = new MiscMaster(),
                MiscFrequencyUnit = new MiscMaster()
            };
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void PreventiveSchedulerHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(PreventiveSchedulerHeader)).Should().BeTrue();
        }

        [Fact]
        public void PreventiveSchedulerHeader_Properties_ShouldBeAssignable()
        {
            var entity = new PreventiveSchedulerHeader
            {
                Id = 1,
                PreventiveSchedulerName = "Monthly PM",
                MachineGroupId = 2,
                MachineGroup = new MachineGroup(),
                DepartmentId = 3,
                MaintenanceCategoryId = 4,
                MiscMaintenanceCategory = new MiscMaster(),
                ScheduleId = 5,
                MiscSchedule = new MiscMaster(),
                FrequencyTypeId = 6,
                MiscFrequencyType = new MiscMaster(),
                FrequencyInterval = 30,
                FrequencyUnitId = 7,
                MiscFrequencyUnit = new MiscMaster(),
                EffectiveDate = new DateOnly(2025, 1, 1),
                GraceDays = 3,
                ReminderWorkOrderDays = 5,
                ReminderMaterialReqDays = 7,
                IsDownTimeRequired = 1,
                DownTimeEstimateHrs = 4.5m,
                UnitId = 10,
                CompanyId = 1
            };
            entity.Id.Should().Be(1);
            entity.PreventiveSchedulerName.Should().Be("Monthly PM");
            entity.MachineGroupId.Should().Be(2);
            entity.FrequencyInterval.Should().Be(30);
            entity.GraceDays.Should().Be(3);
            entity.DownTimeEstimateHrs.Should().Be(4.5m);
        }
    }
}
