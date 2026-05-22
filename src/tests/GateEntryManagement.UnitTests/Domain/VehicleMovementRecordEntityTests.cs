using GateEntryManagement.Domain.Common;
using GateEntryManagement.Domain.Entities;
using static GateEntryManagement.Domain.Common.BaseEntity;

namespace GateEntryManagement.UnitTests.Domain
{
    public class VehicleMovementRecordEntityTests
    {
        [Fact]
        public void VehicleMovementRecord_DefaultIsActive_ShouldBeActive()
        {
            var entity = new VehicleMovementRecord();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void VehicleMovementRecord_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new VehicleMovementRecord();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void VehicleMovementRecord_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(VehicleMovementRecord)).Should().BeTrue();
        }

        [Fact]
        public void VehicleMovementRecord_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new VehicleMovementRecord
            {
                Id = 1,
                VehicleMovementId = "VMR00001",
                VehicleNumber = "KA01AB1234",
                DriverName = "John Doe",
                DriverLicenseNo = "DL123",
                DriverMobileNo = "9876543210",
                TransporterId = 5,
                PurposeOfVisitId = 2,
                ReferenceDocTypeId = 3,
                ReferenceDocNo = "REF001",
                GateInTime = now,
                GateInBy = "admin",
                GateOutTime = now.AddHours(2),
                GateOutBy = "guard",
                StatusId = 1,
                UnitId = 1,
                Remarks = "Test"
            };

            entity.Id.Should().Be(1);
            entity.VehicleMovementId.Should().Be("VMR00001");
            entity.VehicleNumber.Should().Be("KA01AB1234");
            entity.DriverName.Should().Be("John Doe");
            entity.TransporterId.Should().Be(5);
            entity.GateInTime.Should().Be(now);
            entity.GateOutTime.Should().Be(now.AddHours(2));
            entity.StatusId.Should().Be(1);
        }

        [Fact]
        public void VehicleMovementRecord_NullableProperties_ShouldAcceptNull()
        {
            var entity = new VehicleMovementRecord
            {
                VehicleMovementId = null,
                VehicleNumber = null,
                DriverName = null,
                DriverLicenseNo = null,
                DriverMobileNo = null,
                TransporterId = null,
                ReferenceDocTypeId = null,
                ReferenceDocNo = null,
                GateInBy = null,
                GateOutTime = null,
                GateOutBy = null,
                Remarks = null
            };

            entity.TransporterId.Should().BeNull();
            entity.ReferenceDocTypeId.Should().BeNull();
            entity.GateOutTime.Should().BeNull();
        }

        [Fact]
        public void VehicleMovementRecord_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new VehicleMovementRecord
            {
                PurposeOfVisit = new MiscMaster { Id = 1, Code = "DELIVERY" },
                GatePassHeaders = new List<GatePassHdr> { new GatePassHdr { Id = 1 } },
                GateInwardHeaders = new List<GateInwardHdr> { new GateInwardHdr { Id = 1 } }
            };

            entity.PurposeOfVisit.Should().NotBeNull();
            entity.GatePassHeaders.Should().HaveCount(1);
            entity.GateInwardHeaders.Should().HaveCount(1);
        }
    }
}
