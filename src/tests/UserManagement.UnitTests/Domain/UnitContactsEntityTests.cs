using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;

namespace UserManagement.UnitTests.Domain
{
    public class UnitContactsEntityTests
    {
        [Fact]
        public void UnitContacts_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(UnitContacts)).Should().BeFalse();
        }

        [Fact]
        public void UnitContacts_Properties_ShouldBeAssignable()
        {
            var entity = new UnitContacts
            {
                Id = 1,
                UnitId = 5,
                Name = "Jane Smith",
                Designation = "Supervisor",
                Email = "jane@test.com",
                PhoneNo = "9876543210",
                Remarks = "Secondary contact"
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(5);
            entity.Name.Should().Be("Jane Smith");
            entity.Designation.Should().Be("Supervisor");
            entity.Email.Should().Be("jane@test.com");
            entity.PhoneNo.Should().Be("9876543210");
            entity.Remarks.Should().Be("Secondary contact");
        }

        [Fact]
        public void UnitContacts_NullableProperties_ShouldAcceptNull()
        {
            var entity = new UnitContacts
            {
                Name = null,
                Designation = null,
                Email = null,
                PhoneNo = null,
                Remarks = null,
                Unit = null
            };

            entity.Name.Should().BeNull();
            entity.Designation.Should().BeNull();
            entity.Email.Should().BeNull();
            entity.PhoneNo.Should().BeNull();
            entity.Remarks.Should().BeNull();
            entity.Unit.Should().BeNull();
        }

        [Fact]
        public void UnitContacts_NavigationProperty_Unit_ShouldBeAssignable()
        {
            var unit = new Unit { Id = 7 };
            var entity = new UnitContacts { Unit = unit, UnitId = 7 };

            entity.Unit.Should().NotBeNull();
            entity.Unit!.Id.Should().Be(7);
        }
    }
}
