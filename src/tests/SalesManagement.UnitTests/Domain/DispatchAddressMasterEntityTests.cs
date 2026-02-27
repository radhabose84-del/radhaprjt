using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class DispatchAddressMasterEntityTests
    {
        [Fact]
        public void DispatchAddressMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new DispatchAddressMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void DispatchAddressMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DispatchAddressMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void DispatchAddressMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DispatchAddressMaster)).Should().BeTrue();
        }

        [Fact]
        public void DispatchAddressMaster_MandatoryProperties_ShouldBeAssignable()
        {
            var entity = new DispatchAddressMaster
            {
                Id = 1,
                DispatchAddressName = "Test Warehouse",
                AddressLine1 = "123 Main Street",
                CityId = 10,
                StateId = 5,
                CountryId = 1,
                PinCode = "110001"
            };

            entity.Id.Should().Be(1);
            entity.DispatchAddressName.Should().Be("Test Warehouse");
            entity.AddressLine1.Should().Be("123 Main Street");
            entity.CityId.Should().Be(10);
            entity.StateId.Should().Be(5);
            entity.CountryId.Should().Be(1);
            entity.PinCode.Should().Be("110001");
        }

        [Fact]
        public void DispatchAddressMaster_OptionalProperties_ShouldAcceptNull()
        {
            var entity = new DispatchAddressMaster
            {
                AddressLine2 = null,
                ContactPerson = null,
                MobileNumber = null,
                Email = null,
                GSTIN = null,
                Latitude = null,
                Longitude = null
            };

            entity.AddressLine2.Should().BeNull();
            entity.ContactPerson.Should().BeNull();
            entity.MobileNumber.Should().BeNull();
            entity.Email.Should().BeNull();
            entity.GSTIN.Should().BeNull();
            entity.Latitude.Should().BeNull();
            entity.Longitude.Should().BeNull();
        }

        [Fact]
        public void DispatchAddressMaster_OptionalProperties_ShouldAcceptValues()
        {
            var entity = new DispatchAddressMaster
            {
                AddressLine2 = "Near Metro Station",
                ContactPerson = "John Doe",
                MobileNumber = "9876543210",
                Email = "test@example.com",
                GSTIN = "22AAAAA0000A1Z5",
                Latitude = 28.6139m,
                Longitude = 77.2090m
            };

            entity.AddressLine2.Should().Be("Near Metro Station");
            entity.ContactPerson.Should().Be("John Doe");
            entity.MobileNumber.Should().Be("9876543210");
            entity.Email.Should().Be("test@example.com");
            entity.GSTIN.Should().Be("22AAAAA0000A1Z5");
            entity.Latitude.Should().Be(28.6139m);
            entity.Longitude.Should().Be(77.2090m);
        }
    }
}
