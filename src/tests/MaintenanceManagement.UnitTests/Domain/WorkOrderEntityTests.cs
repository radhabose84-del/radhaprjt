using MaintenanceManagement.Domain.Common;
using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class WorkOrderEntityTests
    {
        [Fact]
        public void WorkOrder_ShouldInheritFromCommonEntity()
        {
            typeof(CommonEntity).IsAssignableFrom(typeof(WorkOrder)).Should().BeTrue();
        }

        [Fact]
        public void WorkOrder_DoesNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(WorkOrder)).Should().BeFalse();
        }

        [Fact]
        public void WorkOrder_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new WorkOrder
            {
                Id = 1,
                CompanyId = 10,
                UnitId = 20,
                WorkOrderDocNo = "WO001",
                RequestId = 5,
                PreventiveScheduleId = 3,
                StatusId = 1,
                MiscStatus = new MiscMaster(),
                RootCauseId = 2,
                Remarks = "Urgent repair",
                Image = "image.jpg",
                TotalManPower = 4,
                TotalSpentHours = 8.5m,
                DowntimeStart = now,
                DowntimeEnd = now.AddHours(2)
            };
            entity.Id.Should().Be(1);
            entity.CompanyId.Should().Be(10);
            entity.UnitId.Should().Be(20);
            entity.WorkOrderDocNo.Should().Be("WO001");
            entity.RequestId.Should().Be(5);
            entity.PreventiveScheduleId.Should().Be(3);
            entity.StatusId.Should().Be(1);
            entity.RootCauseId.Should().Be(2);
            entity.Remarks.Should().Be("Urgent repair");
            entity.Image.Should().Be("image.jpg");
            entity.TotalManPower.Should().Be(4);
            entity.TotalSpentHours.Should().Be(8.5m);
            entity.DowntimeStart.Should().Be(now);
            entity.DowntimeEnd.Should().Be(now.AddHours(2));
        }

        [Fact]
        public void WorkOrder_NullableProperties_ShouldAcceptNull()
        {
            var entity = new WorkOrder
            {
                MiscStatus = new MiscMaster(),
                WorkOrderDocNo = null,
                RequestId = null,
                PreventiveScheduleId = null,
                RootCauseId = null,
                Remarks = null,
                Image = null,
                TotalManPower = null,
                TotalSpentHours = null,
                DowntimeStart = null,
                DowntimeEnd = null
            };
            entity.WorkOrderDocNo.Should().BeNull();
            entity.RequestId.Should().BeNull();
            entity.PreventiveScheduleId.Should().BeNull();
            entity.RootCauseId.Should().BeNull();
            entity.Remarks.Should().BeNull();
            entity.Image.Should().BeNull();
            entity.TotalManPower.Should().BeNull();
            entity.TotalSpentHours.Should().BeNull();
            entity.DowntimeStart.Should().BeNull();
            entity.DowntimeEnd.Should().BeNull();
        }

        [Fact]
        public void WorkOrder_CollectionNavigationProperties_ShouldBeAssignable()
        {
            var entity = new WorkOrder
            {
                MiscStatus = new MiscMaster(),
                WorkOrderItems = new List<WorkOrderItem>(),
                WorkOrderActivities = new List<WorkOrderActivity>(),
                WorkOrderSchedules = new List<WorkOrderSchedule>(),
                WorkOrderTechnicians = new List<WorkOrderTechnician>(),
                WorkOrderCheckLists = new List<WorkOrderCheckList>()
            };
            entity.WorkOrderItems.Should().NotBeNull().And.BeEmpty();
            entity.WorkOrderActivities.Should().NotBeNull().And.BeEmpty();
            entity.WorkOrderSchedules.Should().NotBeNull().And.BeEmpty();
            entity.WorkOrderTechnicians.Should().NotBeNull().And.BeEmpty();
            entity.WorkOrderCheckLists.Should().NotBeNull().And.BeEmpty();
        }

        [Fact]
        public void WorkOrder_InheritedAuditFields_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new WorkOrder
            {
                MiscStatus = new MiscMaster(),
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1",
                ModifiedBy = 2,
                ModifiedDate = now.AddHours(1),
                ModifiedByName = "editor",
                ModifiedIP = "10.0.0.1"
            };
            entity.CreatedBy.Should().Be(1);
            entity.CreatedDate.Should().Be(now);
            entity.CreatedByName.Should().Be("admin");
            entity.CreatedIP.Should().Be("127.0.0.1");
            entity.ModifiedBy.Should().Be(2);
            entity.ModifiedDate.Should().Be(now.AddHours(1));
            entity.ModifiedByName.Should().Be("editor");
            entity.ModifiedIP.Should().Be("10.0.0.1");
        }
    }
}
