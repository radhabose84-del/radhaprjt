using MaintenanceManagement.Domain.Entities;
using MaintenanceManagement.Domain.Entities.WorkOrderMaster;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class WorkOrderItemEntityTests
    {
        [Fact]
        public void WorkOrderItem_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(WorkOrderItem)).Should().BeFalse();
        }

        [Fact]
        public void WorkOrderItem_Properties_ShouldBeAssignable()
        {
            var entity = new WorkOrderItem
            {
                Id = 1,
                WorkOrderId = 10,
                StoreTypeId = 3,
                ItemCode = "ITM001",
                OldItemCode = "OLD001",
                ItemName = "Bearing",
                AvailableQty = 50,
                UsedQty = 5,
                ScarpQty = 2,
                ToSubStoreQty = 3,
                Image = "item.jpg",
                Rate = 150.75m,
                DepartmentId = 7
            };
            entity.Id.Should().Be(1);
            entity.WorkOrderId.Should().Be(10);
            entity.StoreTypeId.Should().Be(3);
            entity.ItemCode.Should().Be("ITM001");
            entity.OldItemCode.Should().Be("OLD001");
            entity.ItemName.Should().Be("Bearing");
            entity.AvailableQty.Should().Be(50);
            entity.UsedQty.Should().Be(5);
            entity.ScarpQty.Should().Be(2);
            entity.ToSubStoreQty.Should().Be(3);
            entity.Image.Should().Be("item.jpg");
            entity.Rate.Should().Be(150.75m);
            entity.DepartmentId.Should().Be(7);
        }

        [Fact]
        public void WorkOrderItem_NullableProperties_ShouldAcceptNull()
        {
            var entity = new WorkOrderItem
            {
                WorkOrderId = null,
                StoreTypeId = null,
                ItemCode = null,
                OldItemCode = null,
                ItemName = null,
                ScarpQty = null,
                ToSubStoreQty = null,
                Image = null,
                Rate = null,
                DepartmentId = null
            };
            entity.WorkOrderId.Should().BeNull();
            entity.StoreTypeId.Should().BeNull();
            entity.ItemCode.Should().BeNull();
            entity.OldItemCode.Should().BeNull();
            entity.ItemName.Should().BeNull();
            entity.ScarpQty.Should().BeNull();
            entity.ToSubStoreQty.Should().BeNull();
            entity.Image.Should().BeNull();
            entity.Rate.Should().BeNull();
            entity.DepartmentId.Should().BeNull();
        }

        [Fact]
        public void WorkOrderItem_NavigationProperties_ShouldBeAssignable()
        {
            var workOrder = new WorkOrder { MiscStatus = new MiscMaster() };
            var miscStoreType = new MiscMaster();
            var entity = new WorkOrderItem
            {
                WOItem = workOrder,
                MiscStoreType = miscStoreType
            };
            entity.WOItem.Should().BeSameAs(workOrder);
            entity.MiscStoreType.Should().BeSameAs(miscStoreType);
        }
    }
}
