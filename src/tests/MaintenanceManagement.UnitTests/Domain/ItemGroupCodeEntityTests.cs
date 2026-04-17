using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class ItemGroupCodeEntityTests
    {
        [Fact]
        public void ItemGroupCode_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(ItemGroupCode)).Should().BeFalse();
        }

        [Fact]
        public void ItemGroupCode_Properties_ShouldBeAssignable()
        {
            var entity = new ItemGroupCode
            {
                GroupCode = "GRP001",
                GroupName = "Electrical"
            };
            entity.GroupCode.Should().Be("GRP001");
            entity.GroupName.Should().Be("Electrical");
        }

        [Fact]
        public void ItemGroupCode_NullableProperties_ShouldAcceptNull()
        {
            var entity = new ItemGroupCode
            {
                GroupCode = null,
                GroupName = null
            };
            entity.GroupCode.Should().BeNull();
            entity.GroupName.Should().BeNull();
        }
    }
}
