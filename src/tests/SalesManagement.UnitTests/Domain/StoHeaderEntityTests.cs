using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class StoHeaderEntityTests
    {
        [Fact]
        public void StoHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new StoHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void StoHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new StoHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void StoHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(StoHeader)).Should().BeTrue();
        }

        [Fact]
        public void StoHeader_Properties_ShouldBeAssignable()
        {
            var entity = new StoHeader
            {
                Id = 1,
                StoNumber = "STO001",
                DocumentDate = new DateOnly(2026, 1, 1),
                ExpectedDeliveryDate = new DateOnly(2026, 1, 10),
                StoTypeId = 2,
                MovementTypeId = 3,
                SupplyingPlantId = 4,
                SupplyingStorageLocationId = 5,
                ReceivingPlantId = 6,
                ReceivingStorageLocationId = 7,
                Remarks = "Test",
                HeaderStatusId = 8
            };

            entity.Id.Should().Be(1);
            entity.StoNumber.Should().Be("STO001");
            entity.StoTypeId.Should().Be(2);
            entity.MovementTypeId.Should().Be(3);
            entity.SupplyingPlantId.Should().Be(4);
            entity.ReceivingPlantId.Should().Be(6);
            entity.HeaderStatusId.Should().Be(8);
        }

        [Fact]
        public void StoHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new StoHeader
            {
                StoNumber = null,
                Remarks = null,
                HeaderStatusId = null
            };

            entity.StoNumber.Should().BeNull();
            entity.Remarks.Should().BeNull();
            entity.HeaderStatusId.Should().BeNull();
        }

        [Fact]
        public void StoHeader_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new StoHeader
            {
                StoDetails = new List<StoDetail>(),
                DeliveryChallanHeaders = new List<DeliveryChallanHeader>()
            };

            entity.StoDetails.Should().NotBeNull();
            entity.DeliveryChallanHeaders.Should().NotBeNull();
        }
    }
}
