using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class WorkOrderTechnicianEntityTests
    {
        [Fact]
        public void WorkOrderTechnician_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(WorkOrderTechnician)).Should().BeFalse();
        }

        [Fact]
        public void WorkOrderTechnician_Properties_ShouldBeAssignable()
        {
            var entity = new WorkOrderTechnician
            {
                Id = 1,
                WorkOrderId = 10,
                TechnicianId = 5,
                OldTechnicianId = 3,
                SourceId = 2,
                TechnicianName = "John Doe",
                HoursSpent = 4,
                MinutesSpent = 30
            };
            entity.Id.Should().Be(1);
            entity.WorkOrderId.Should().Be(10);
            entity.TechnicianId.Should().Be(5);
            entity.OldTechnicianId.Should().Be(3);
            entity.SourceId.Should().Be(2);
            entity.TechnicianName.Should().Be("John Doe");
            entity.HoursSpent.Should().Be(4);
            entity.MinutesSpent.Should().Be(30);
        }

        [Fact]
        public void WorkOrderTechnician_NullableProperties_ShouldAcceptNull()
        {
            var entity = new WorkOrderTechnician
            {
                Id = null,
                WorkOrderId = null,
                TechnicianName = null
            };
            entity.Id.Should().BeNull();
            entity.WorkOrderId.Should().BeNull();
            entity.TechnicianName.Should().BeNull();
        }

        [Fact]
        public void WorkOrderTechnician_NavigationProperties_ShouldBeAssignable()
        {
            var workOrder = new WorkOrder { MiscStatus = new MiscMaster() };
            var miscSource = new MiscMaster();
            var entity = new WorkOrderTechnician
            {
                WOTechnician = workOrder,
                MiscSource = miscSource
            };
            entity.WOTechnician.Should().BeSameAs(workOrder);
            entity.MiscSource.Should().BeSameAs(miscSource);
        }
    }
}
