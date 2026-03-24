using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.UnitTests.Domain
{
    public class WarehouseItemGroupMappingEntityTests
    {
        [Fact]
        public void WarehouseItemGroupMapping_DefaultIsActive_ShouldBeActive()
        {
            var entity = new WarehouseItemGroupMapping();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void WarehouseItemGroupMapping_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new WarehouseItemGroupMapping();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void WarehouseItemGroupMapping_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(WarehouseItemGroupMapping)).Should().BeTrue();
        }

        [Fact]
        public void WarehouseItemGroupMapping_Properties_ShouldBeAssignable()
        {
            var entity = new WarehouseItemGroupMapping
            {
                Id = 1,
                WarehouseId = 5,
                ItemGroupId = 10
            };

            entity.Id.Should().Be(1);
            entity.WarehouseId.Should().Be(5);
            entity.ItemGroupId.Should().Be(10);
        }
    }
}
