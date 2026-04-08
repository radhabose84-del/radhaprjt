using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class StoReceiptHeaderEntityTests
    {
        [Fact]
        public void StoReceiptHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new StoReceiptHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void StoReceiptHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new StoReceiptHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void StoReceiptHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(StoReceiptHeader)).Should().BeTrue();
        }

        [Fact]
        public void StoReceiptHeader_Properties_ShouldBeAssignable()
        {
            var entity = new StoReceiptHeader
            {
                Id = 1,
                StoReceiptNumber = "SR001",
                StoReceiptDate = new DateOnly(2026, 1, 1),
                DeliveryChallanHeaderId = 2,
                ReceivingPlantId = 3,
                ReceivingStorageLocationId = 4,
                BinId = 5,
                VehicleNumber = "KA01AB1234",
                Remarks = "Test",
                StatusId = 6
            };

            entity.Id.Should().Be(1);
            entity.StoReceiptNumber.Should().Be("SR001");
            entity.DeliveryChallanHeaderId.Should().Be(2);
            entity.ReceivingPlantId.Should().Be(3);
            entity.StatusId.Should().Be(6);
        }

        [Fact]
        public void StoReceiptHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new StoReceiptHeader
            {
                StoReceiptNumber = null,
                BinId = null,
                VehicleNumber = null,
                Remarks = null
            };

            entity.StoReceiptNumber.Should().BeNull();
            entity.BinId.Should().BeNull();
        }

        [Fact]
        public void StoReceiptHeader_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new StoReceiptHeader
            {
                DeliveryChallanHeader = new DeliveryChallanHeader { Id = 1 },
                StoReceiptDetails = new List<StoReceiptDetail>()
            };

            entity.DeliveryChallanHeader.Should().NotBeNull();
            entity.StoReceiptDetails.Should().NotBeNull();
        }
    }
}
