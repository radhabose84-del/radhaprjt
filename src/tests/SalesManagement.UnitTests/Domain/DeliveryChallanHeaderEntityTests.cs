using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class DeliveryChallanHeaderEntityTests
    {
        [Fact]
        public void DeliveryChallanHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DeliveryChallanHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DeliveryChallanHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DeliveryChallanHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DeliveryChallanHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DeliveryChallanHeader)).Should().BeTrue();
        }

        [Fact]
        public void DeliveryChallanHeader_Properties_ShouldBeAssignable()
        {
            var entity = new DeliveryChallanHeader
            {
                Id = 1,
                DeliveryNumber = "DC001",
                DeliveryDate = new DateOnly(2026, 1, 1),
                StoHeaderId = 10,
                FromPlantId = 2,
                FromStorageLocationId = 3,
                ToPlantId = 4,
                ToStorageLocationId = 5,
                TransporterId = 6,
                VehicleNumber = "KA01AB1234",
                TransportDistance = 100.5m,
                DeliveryValue = 5000m,
                ConsignmentValue = 4500m,
                StatusId = 7,
                Remarks = "Test remarks",
                GEFlag = false
            };

            entity.Id.Should().Be(1);
            entity.DeliveryNumber.Should().Be("DC001");
            entity.StoHeaderId.Should().Be(10);
            entity.FromPlantId.Should().Be(2);
            entity.ToPlantId.Should().Be(4);
            entity.TransporterId.Should().Be(6);
            entity.DeliveryValue.Should().Be(5000m);
            entity.ConsignmentValue.Should().Be(4500m);
            entity.GEFlag.Should().BeFalse();
        }

        [Fact]
        public void DeliveryChallanHeader_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new DeliveryChallanHeader
            {
                StoHeader = new StoHeader { Id = 1 },
                DeliveryChallanDetails = new List<DeliveryChallanDetail>()
            };

            entity.StoHeader.Should().NotBeNull();
            entity.DeliveryChallanDetails.Should().NotBeNull();
        }
    }
}
