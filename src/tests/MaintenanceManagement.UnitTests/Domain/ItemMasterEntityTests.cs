using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class ItemMasterEntityTests
    {
        [Fact]
        public void ItemMaster_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ItemMaster)).Should().BeFalse();
        }

        [Fact]
        public void ItemMaster_Properties_ShouldBeAssignable()
        {
            var entity = new ItemMaster
            {
                ItemCode = "ITM001",
                ItemName = "Bearing",
                Uom = "NOS",
                Description = "Ball bearing 6205"
            };
            entity.ItemCode.Should().Be("ITM001");
            entity.ItemName.Should().Be("Bearing");
            entity.Uom.Should().Be("NOS");
            entity.Description.Should().Be("Ball bearing 6205");
        }

        [Fact]
        public void ItemMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemMaster
            {
                ItemCode = null,
                ItemName = null,
                Uom = null,
                Description = null
            };
            entity.ItemCode.Should().BeNull();
            entity.ItemName.Should().BeNull();
            entity.Uom.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
