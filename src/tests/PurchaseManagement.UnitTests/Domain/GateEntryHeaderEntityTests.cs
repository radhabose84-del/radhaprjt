using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Domain
{
    public class GateEntryHeaderEntityTests
    {
        [Fact]
        public void GateEntryHeader_DefaultIsActive_ShouldBeActive()
        {
            var entity = new GateEntryHeader();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void GateEntryHeader_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new GateEntryHeader();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void GateEntryHeader_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(GateEntryHeader)).Should().BeTrue();
        }

        [Fact]
        public void GateEntryHeader_Properties_ShouldBeAssignable()
        {
            var date = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var entity = new GateEntryHeader
            {
                Id = 1,
                GateEntryNo = "GE001",
                GateEntryDate = date,
                UnitId = 10,
                PartyId = 20,
                VehicleNumber = "TN01AB1234",
                DriverName = "Test Driver",
                GrossWeight = 5000m,
                TareWeight = 1000m,
                NetWeight = 4000m,
                ImagePath = "/path/img.jpg",
                Remarks = "Test",
                ReceivingTypeId = 5
            };

            entity.Id.Should().Be(1);
            entity.GateEntryNo.Should().Be("GE001");
            entity.GateEntryDate.Should().Be(date);
            entity.UnitId.Should().Be(10);
            entity.VehicleNumber.Should().Be("TN01AB1234");
            entity.GrossWeight.Should().Be(5000m);
            entity.NetWeight.Should().Be(4000m);
        }

        [Fact]
        public void GateEntryHeader_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GateEntryHeader
            {
                GateEntryNo = null,
                DriverName = null,
                GrossWeight = null,
                TareWeight = null,
                NetWeight = null,
                ImagePath = null,
                Remarks = null,
                GateEntryReceivingType = null,
                GateEntryDetails = null,
                GrnGateEntries = null
            };

            entity.GateEntryNo.Should().BeNull();
            entity.DriverName.Should().BeNull();
            entity.NetWeight.Should().BeNull();
            entity.GateEntryDetails.Should().BeNull();
        }
    }
}
