using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class WorkOrderScheduleEntityTests
    {
        [Fact]
        public void WorkOrderSchedule_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(WorkOrderSchedule)).Should().BeFalse();
        }

        [Fact]
        public void WorkOrderSchedule_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new WorkOrderSchedule
            {
                Id = 1,
                WorkOrderId = 10,
                StartTime = now,
                EndTime = now.AddHours(4),
                IsCompleted = 1,
                StatusId = 2,
                MiscStatus = new MiscMaster()
            };
            entity.Id.Should().Be(1);
            entity.WorkOrderId.Should().Be(10);
            entity.StartTime.Should().Be(now);
            entity.EndTime.Should().Be(now.AddHours(4));
            entity.IsCompleted.Should().Be(1);
            entity.StatusId.Should().Be(2);
            entity.MiscStatus.Should().NotBeNull();
        }

        [Fact]
        public void WorkOrderSchedule_DefaultIsCompleted_ShouldBeZero()
        {
            var entity = new WorkOrderSchedule
            {
                MiscStatus = new MiscMaster()
            };
            entity.IsCompleted.Should().Be(0);
        }

        [Fact]
        public void WorkOrderSchedule_NullableProperties_ShouldAcceptNull()
        {
            var entity = new WorkOrderSchedule
            {
                MiscStatus = new MiscMaster(),
                EndTime = null,
                IsCompleted = null
            };
            entity.EndTime.Should().BeNull();
            entity.IsCompleted.Should().BeNull();
        }

        [Fact]
        public void WorkOrderSchedule_NavigationProperties_ShouldBeAssignable()
        {
            var workOrder = new WorkOrder { MiscStatus = new MiscMaster() };
            var miscStatus = new MiscMaster();
            var entity = new WorkOrderSchedule
            {
                WOSchedule = workOrder,
                MiscStatus = miscStatus
            };
            entity.WOSchedule.Should().BeSameAs(workOrder);
            entity.MiscStatus.Should().BeSameAs(miscStatus);
        }
    }
}
