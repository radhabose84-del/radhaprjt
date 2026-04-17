using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Domain
{
    public class PartyAddressEntityTests
    {
        [Fact]
        public void PartyAddress_ShouldNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(PartyAddress)).Should().BeFalse();
        }

        [Fact]
        public void PartyAddress_Properties_ShouldBeAssignable()
        {
            var entity = new PartyAddress
            {
                Id = 1,
                PartyId = 10,
                AddressType = "Billing",
                AddressLine1 = "123 Main Street",
                AddressLine2 = "Suite 100",
                CityId = 5,
                StateId = 3,
                PostalCode = "600001",
                CountryId = 1
            };

            entity.Id.Should().Be(1);
            entity.PartyId.Should().Be(10);
            entity.AddressType.Should().Be("Billing");
            entity.AddressLine1.Should().Be("123 Main Street");
            entity.AddressLine2.Should().Be("Suite 100");
            entity.CityId.Should().Be(5);
            entity.StateId.Should().Be(3);
            entity.PostalCode.Should().Be("600001");
            entity.CountryId.Should().Be(1);
        }

        [Fact]
        public void PartyAddress_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PartyAddress
            {
                AddressType = null,
                AddressLine1 = null,
                AddressLine2 = null,
                PostalCode = null
            };

            entity.AddressType.Should().BeNull();
            entity.AddressLine1.Should().BeNull();
            entity.AddressLine2.Should().BeNull();
            entity.PostalCode.Should().BeNull();
        }

        [Fact]
        public void PartyAddress_NavigationProperty_ShouldBeAssignable()
        {
            var party = new PartyMaster { Id = 10, PartyCode = "P001" };
            var entity = new PartyAddress
            {
                PartyAddressId = party
            };

            entity.PartyAddressId.Should().NotBeNull();
            entity.PartyAddressId.Id.Should().Be(10);
        }
    }
}
