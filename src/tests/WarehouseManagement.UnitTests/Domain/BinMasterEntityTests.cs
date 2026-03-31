using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Entities;
using static WarehouseManagement.Domain.Common.BaseEntity;

namespace WarehouseManagement.UnitTests.Domain
{
    public class BinMasterEntityTests
    {
        [Fact]
        public void BinMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new BinMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void BinMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new BinMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void BinMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(BinMaster)).Should().BeTrue();
        }

        [Fact]
        public void BinMaster_Properties_ShouldBeAssignable()
        {
            var entity = new BinMaster
            {
                Id = 1,
                WarehouseId = 5,
                RackId = 3,
                BinCode = "BIN001",
                BinName = "Bin A",
                BinCapacity = 100.5m,
                CapacityUOMId = 2
            };

            entity.Id.Should().Be(1);
            entity.WarehouseId.Should().Be(5);
            entity.RackId.Should().Be(3);
            entity.BinCode.Should().Be("BIN001");
            entity.BinCapacity.Should().Be(100.5m);
        }

        [Fact]
        public void BinMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new BinMaster
            {
                RackId = null,
                BinName = null,
                Rack = null
            };

            entity.RackId.Should().BeNull();
            entity.BinName.Should().BeNull();
            entity.Rack.Should().BeNull();
        }
    }
}
