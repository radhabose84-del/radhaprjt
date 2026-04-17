using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class UnitAddressEntityTests
    {
        [Fact]
        public void UnitAddress_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UnitAddress)).Should().BeFalse();
        }

        [Fact]
        public void UnitAddress_Properties_ShouldBeAssignable()
        {
            var entity = new UnitAddress
            {
                Id = 1,
                UnitId = 5,
                CountryId = 1,
                StateId = 10,
                CityId = 20,
                AddressLine1 = "456 Industrial Area",
                AddressLine2 = "Block B",
                PinCode = 600001,
                ContactNumber = "9876543210",
                AlternateNumber = "0441234567"
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(5);
            entity.CountryId.Should().Be(1);
            entity.StateId.Should().Be(10);
            entity.CityId.Should().Be(20);
            entity.AddressLine1.Should().Be("456 Industrial Area");
            entity.AddressLine2.Should().Be("Block B");
            entity.PinCode.Should().Be(600001);
            entity.ContactNumber.Should().Be("9876543210");
            entity.AlternateNumber.Should().Be("0441234567");
        }

        [Fact]
        public void UnitAddress_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UnitAddress
            {
                AddressLine1 = null,
                AddressLine2 = null,
                ContactNumber = null,
                AlternateNumber = null,
                Unit = null
            };

            entity.AddressLine1.Should().BeNull();
            entity.AddressLine2.Should().BeNull();
            entity.ContactNumber.Should().BeNull();
            entity.AlternateNumber.Should().BeNull();
            entity.Unit.Should().BeNull();
        }

        [Fact]
        public void UnitAddress_NavigationProperty_Unit_ShouldBeAssignable()
        {
            var unit = new Unit { Id = 3 };
            var entity = new UnitAddress { Unit = unit, UnitId = 3 };

            entity.Unit.Should().NotBeNull();
            entity.Unit!.Id.Should().Be(3);
        }
    }
}
