using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class WorkOrderActivityEntityTests
    {
        [Fact]
        public void WorkOrderActivity_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(WorkOrderActivity)).Should().BeFalse();
        }

        [Fact]
        public void WorkOrderActivity_Properties_ShouldBeAssignable()
        {
            var entity = new WorkOrderActivity
            {
                Id = 1,
                WorkOrderId = 10,
                ActivityId = 5,
                Description = "Lubrication completed"
            };
            entity.Id.Should().Be(1);
            entity.WorkOrderId.Should().Be(10);
            entity.ActivityId.Should().Be(5);
            entity.Description.Should().Be("Lubrication completed");
        }

        [Fact]
        public void WorkOrderActivity_NavigationProperties_ShouldBeAssignable()
        {
            var workOrder = new WorkOrder { MiscStatus = new MiscMaster() };
            var activityMaster = new ActivityMaster();
            var entity = new WorkOrderActivity
            {
                WOActivity = workOrder,
                ActivityMaster = activityMaster
            };
            entity.WOActivity.Should().BeSameAs(workOrder);
            entity.ActivityMaster.Should().BeSameAs(activityMaster);
        }

        [Fact]
        public void WorkOrderActivity_NullableProperties_ShouldAcceptNull()
        {
            var entity = new WorkOrderActivity
            {
                Description = null
            };
            entity.Description.Should().BeNull();
        }
    }
}
