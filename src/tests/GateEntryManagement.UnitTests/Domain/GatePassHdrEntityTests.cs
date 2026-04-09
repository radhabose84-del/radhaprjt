using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.UnitTests.Domain
{
    public class GatePassHdrEntityTests
    {
        [Fact]
        public void GatePassHdr_DefaultIsActive_ShouldBeActive()
        {
            var entity = new GatePassHdr();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void GatePassHdr_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new GatePassHdr();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void GatePassHdr_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(GatePassHdr)).Should().BeTrue();
        }

        [Fact]
        public void GatePassHdr_Properties_ShouldBeAssignable()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var entity = new GatePassHdr
            {
                Id = 1,
                GatePassNo = "GP00001",
                GatePassDate = today,
                VehicleMovementRecordId = 5,
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverMobile = "9876543210",
                TransporterName = "Test Transporter",
                UnitId = 1,
                TotalItems = 5,
                TotalDocumentQty = 100m,
                TotalDispatchQty = 100m,
                ReturnableItems = 2,
                TotalValue = 50000m,
                Remarks = "Test"
            };

            entity.Id.Should().Be(1);
            entity.GatePassNo.Should().Be("GP00001");
            entity.GatePassDate.Should().Be(today);
            entity.TotalItems.Should().Be(5);
            entity.TotalDocumentQty.Should().Be(100m);
            entity.TotalValue.Should().Be(50000m);
            entity.ReturnableItems.Should().Be(2);
        }

        [Fact]
        public void GatePassHdr_NullableProperties_ShouldAcceptNull()
        {
            var entity = new GatePassHdr
            {
                GatePassNo = null,
                VehicleNumber = null,
                DriverName = null,
                DriverMobile = null,
                TransporterName = null,
                ReturnableItems = null,
                Remarks = null
            };

            entity.GatePassNo.Should().BeNull();
            entity.ReturnableItems.Should().BeNull();
        }

        [Fact]
        public void GatePassHdr_DetailCollection_ShouldBeAssignable()
        {
            var entity = new GatePassHdr
            {
                GatePassDetails = new List<GatePassDtl>
                {
                    new GatePassDtl { Id = 1, DocTypeId = 1, DocNo = "PO001", TotalQty = 50m }
                }
            };

            entity.GatePassDetails.Should().HaveCount(1);
        }
    }
}
