using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class ActivityMachineGroupEntityTests
    {
        [Fact]
        public void ActivityMachineGroup_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ActivityMachineGroup)).Should().BeFalse();
        }

        [Fact]
        public void ActivityMachineGroup_Properties_ShouldBeAssignable()
        {
            var entity = new ActivityMachineGroup
            {
                Id = 1,
                ActivityMasterId = 10,
                MachineGroupId = 20
            };
            entity.Id.Should().Be(1);
            entity.ActivityMasterId.Should().Be(10);
            entity.MachineGroupId.Should().Be(20);
        }

        [Fact]
        public void ActivityMachineGroup_NavigationProperties_ShouldBeAssignable()
        {
            var activity = new ActivityMaster();
            var machineGroup = new MachineGroup();
            var entity = new ActivityMachineGroup
            {
                ActivityMaster = activity,
                MachineGroup = machineGroup
            };
            entity.ActivityMaster.Should().BeSameAs(activity);
            entity.MachineGroup.Should().BeSameAs(machineGroup);
        }

        [Fact]
        public void ActivityMachineGroup_NullableNavigationProperties_ShouldAcceptNull()
        {
            var entity = new ActivityMachineGroup
            {
                ActivityMaster = null,
                MachineGroup = null
            };
            entity.ActivityMaster.Should().BeNull();
            entity.MachineGroup.Should().BeNull();
        }
    }
}
