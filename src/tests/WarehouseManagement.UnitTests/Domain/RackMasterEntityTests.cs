using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.UnitTests.Domain
{
    public class RackMasterEntityTests
    {
        [Fact]
        public void RackMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new RackMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void RackMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new RackMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void RackMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(RackMaster)).Should().BeTrue();
        }

        [Fact]
        public void RackMaster_Properties_ShouldBeAssignable()
        {
            var entity = new RackMaster
            {
                Id = 1,
                WarehouseId = 5,
                RackCode = "RK001",
                RackName = "Rack A",
                FloorId = 1,
                AisleId = 2,
                RackLevelId = 3,
                MaxCapacity = 500m,
                CapacityUOMId = 1,
                RackWidth = 2.5m,
                RackHeight = 3.0m,
                DimensionUOMId = 2
            };

            entity.Id.Should().Be(1);
            entity.WarehouseId.Should().Be(5);
            entity.RackCode.Should().Be("RK001");
            entity.MaxCapacity.Should().Be(500m);
        }

        [Fact]
        public void RackMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new RackMaster
            {
                FloorId = null,
                AisleId = null,
                RackLevelId = null,
                MaxCapacity = null,
                CapacityUOMId = null,
                RackWidth = null,
                RackHeight = null,
                DimensionUOMId = null,
                RackName = null,
                Warehouse = null
            };

            entity.FloorId.Should().BeNull();
            entity.AisleId.Should().BeNull();
            entity.Warehouse.Should().BeNull();
        }

        [Fact]
        public void RackMaster_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new RackMaster();
            entity.Bins.Should().NotBeNull();
        }
    }
}
