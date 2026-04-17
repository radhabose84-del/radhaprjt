using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class PreventiveSchedulerActivityEntityTests
    {
        [Fact]
        public void PreventiveSchedulerActivity_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(PreventiveSchedulerActivity)).Should().BeFalse();
        }

        private static PreventiveSchedulerHeader CreateHeader() =>
            new PreventiveSchedulerHeader
            {
                MachineGroup = new MachineGroup(),
                MiscMaintenanceCategory = new MiscMaster(),
                MiscSchedule = new MiscMaster(),
                MiscFrequencyType = new MiscMaster(),
                MiscFrequencyUnit = new MiscMaster()
            };

        [Fact]
        public void PreventiveSchedulerActivity_Properties_ShouldBeAssignable()
        {
            var header = CreateHeader();
            var activity = new ActivityMaster();
            var entity = new PreventiveSchedulerActivity
            {
                Id = 1,
                PreventiveSchedulerHeaderId = 10,
                PreventiveScheduler = header,
                ActivityId = 5,
                Activity = activity
            };
            entity.Id.Should().Be(1);
            entity.PreventiveSchedulerHeaderId.Should().Be(10);
            entity.PreventiveScheduler.Should().BeSameAs(header);
            entity.ActivityId.Should().Be(5);
            entity.Activity.Should().BeSameAs(activity);
        }
    }
}
