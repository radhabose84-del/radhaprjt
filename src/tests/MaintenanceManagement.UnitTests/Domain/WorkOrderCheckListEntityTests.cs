using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class WorkOrderCheckListEntityTests
    {
        [Fact]
        public void WorkOrderCheckList_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(WorkOrderCheckList)).Should().BeFalse();
        }

        [Fact]
        public void WorkOrderCheckList_Properties_ShouldBeAssignable()
        {
            var entity = new WorkOrderCheckList
            {
                Id = 1,
                WorkOrderId = 10,
                CheckListId = 5,
                ISCompleted = 1,
                Description = "Check oil level"
            };
            entity.Id.Should().Be(1);
            entity.WorkOrderId.Should().Be(10);
            entity.CheckListId.Should().Be(5);
            entity.ISCompleted.Should().Be(1);
            entity.Description.Should().Be("Check oil level");
        }

        [Fact]
        public void WorkOrderCheckList_NavigationProperties_ShouldBeAssignable()
        {
            var workOrder = new WorkOrder { MiscStatus = new MiscMaster() };
            var checkListMaster = new ActivityCheckListMaster();
            var entity = new WorkOrderCheckList
            {
                WOCheckList = workOrder,
                CheckListMaster = checkListMaster
            };
            entity.WOCheckList.Should().BeSameAs(workOrder);
            entity.CheckListMaster.Should().BeSameAs(checkListMaster);
        }

        [Fact]
        public void WorkOrderCheckList_NullableProperties_ShouldAcceptNull()
        {
            var entity = new WorkOrderCheckList
            {
                Description = null
            };
            entity.Description.Should().BeNull();
        }
    }
}
