using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Arrival;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class ArrivalHeaderEntityTests
    {
        [Fact]
        public void ArrivalHeader_DefaultIsActive_ShouldBeActive()
        {
            new ArrivalHeader().IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void ArrivalHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            new ArrivalHeader().IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void ArrivalHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(ArrivalHeader)).Should().BeTrue();
        }

        [Fact]
        public void ArrivalHeader_Properties_ShouldBeAssignable()
        {
            var entity = new ArrivalHeader
            {
                Id = 1,
                ArrivalNumber = "ARV-2025-0006",
                VehicleNumber = "TN-38-BC-4521",
                GrossWeight = 30000m,
                TareWeight = 10000m,
                NetWeight = 20000m,
                PartyWeight = 19900m,
                WeightDifference = -100m,
                GstPercentage = 5m
            };

            entity.Id.Should().Be(1);
            entity.ArrivalNumber.Should().Be("ARV-2025-0006");
            entity.NetWeight.Should().Be(20000m);
            entity.WeightDifference.Should().Be(-100m);
            entity.GstPercentage.Should().Be(5m);
        }
    }
}
