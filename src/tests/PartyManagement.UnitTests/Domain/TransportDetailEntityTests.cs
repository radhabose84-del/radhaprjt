using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Domain
{
    public class TransportDetailEntityTests
    {
        [Fact]
        public void TransportDetail_ShouldNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(TransportDetail)).Should().BeFalse();
        }

        [Fact]
        public void TransportDetail_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new TransportDetail
            {
                Id = 1,
                PartyId = 10,
                TransportModeId = 2,
                VehicleTypeId = 3,
                DefaultFreightTypeId = 4,
                DefaultFreightRate = 150.50m,
                LicenseNo = "LIC12345",
                LicenseExpiryDate = now.AddYears(1),
                VehicleNo = "TN01AB1234",
                Status = 1
            };

            entity.Id.Should().Be(1);
            entity.PartyId.Should().Be(10);
            entity.TransportModeId.Should().Be(2);
            entity.VehicleTypeId.Should().Be(3);
            entity.DefaultFreightTypeId.Should().Be(4);
            entity.DefaultFreightRate.Should().Be(150.50m);
            entity.LicenseNo.Should().Be("LIC12345");
            entity.VehicleNo.Should().Be("TN01AB1234");
            entity.Status.Should().Be(1);
        }

        [Fact]
        public void TransportDetail_NullableProperties_ShouldAcceptNull()
        {
            var entity = new TransportDetail
            {
                TransportModeId = null,
                VehicleTypeId = null,
                DefaultFreightTypeId = null,
                DefaultFreightRate = null,
                LicenseNo = null,
                LicenseExpiryDate = null,
                VehicleNo = null
            };

            entity.TransportModeId.Should().BeNull();
            entity.VehicleTypeId.Should().BeNull();
            entity.DefaultFreightTypeId.Should().BeNull();
            entity.DefaultFreightRate.Should().BeNull();
            entity.LicenseNo.Should().BeNull();
            entity.LicenseExpiryDate.Should().BeNull();
            entity.VehicleNo.Should().BeNull();
        }

        [Fact]
        public void TransportDetail_DefaultStatus_ShouldBeOne()
        {
            var entity = new TransportDetail();
            entity.Status.Should().Be(1);
        }

        [Fact]
        public void TransportDetail_NavigationProperties_ShouldBeAssignable()
        {
            var party = new PartyMaster { Id = 10 };
            var transportMode = new MiscMaster { Id = 2 };
            var vehicleType = new MiscMaster { Id = 3 };
            var freightType = new MiscMaster { Id = 4 };

            var entity = new TransportDetail
            {
                PartyMaster = party,
                TransportModeMisc = transportMode,
                VehicleTypeMisc = vehicleType,
                DefaultFreightTypeMisc = freightType
            };

            entity.PartyMaster.Should().NotBeNull();
            entity.TransportModeMisc.Should().NotBeNull();
            entity.VehicleTypeMisc.Should().NotBeNull();
            entity.DefaultFreightTypeMisc.Should().NotBeNull();
        }
    }
}
