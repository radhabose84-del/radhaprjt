using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;
using DomainUnit = UserManagement.Domain.Entities.Unit;

namespace UserManagement.UnitTests.Domain.UnitEntity
{
    public class UnitEntityTests
    {
        [Fact]
        public void Unit_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new DomainUnit();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Unit_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new DomainUnit();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Unit_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(DomainUnit)).Should().BeTrue();
        }

        [Fact]
        public void Unit_Properties_ShouldBeAssignable()
        {
            var entity = new DomainUnit
            {
                Id = 1,
                UnitName = "Test Unit",
                ShortName = "TU",
                CompanyId = 10,
                DivisionId = 5,
                UnitHeadName = "Head Person",
                CINNO = "CIN123",
                OldUnitId = "OLD001",
                IsMaintenanceStopStart = true,
                SpindlesCapacity = 200,
                UnitTypeId = 3
            };

            entity.Id.Should().Be(1);
            entity.UnitName.Should().Be("Test Unit");
            entity.ShortName.Should().Be("TU");
            entity.CompanyId.Should().Be(10);
            entity.DivisionId.Should().Be(5);
            entity.UnitHeadName.Should().Be("Head Person");
            entity.CINNO.Should().Be("CIN123");
            entity.OldUnitId.Should().Be("OLD001");
            entity.IsMaintenanceStopStart.Should().BeTrue();
            entity.SpindlesCapacity.Should().Be(200);
            entity.UnitTypeId.Should().Be(3);
        }

        [Fact]
        public void Unit_NullableProperties_ShouldAcceptNull()
        {
            var entity = new DomainUnit
            {
                UnitName = null,
                ShortName = null,
                UnitHeadName = null,
                CINNO = null,
                OldUnitId = null,
                SpindlesCapacity = null,
                UnitAddress = null,
                UnitContacts = null,
                Company = null,
                Division = null,
                UnitType = null,
                UnitTypeName = null,
                UserUnits = null,
                CustomFieldUnits = null
            };

            entity.UnitName.Should().BeNull();
            entity.ShortName.Should().BeNull();
            entity.UnitHeadName.Should().BeNull();
            entity.CINNO.Should().BeNull();
            entity.OldUnitId.Should().BeNull();
            entity.SpindlesCapacity.Should().BeNull();
            entity.UnitAddress.Should().BeNull();
            entity.UnitContacts.Should().BeNull();
            entity.Company.Should().BeNull();
            entity.Division.Should().BeNull();
            entity.UnitType.Should().BeNull();
            entity.UnitTypeName.Should().BeNull();
            entity.UserUnits.Should().BeNull();
            entity.CustomFieldUnits.Should().BeNull();
        }

        [Fact]
        public void Unit_NavigationProperty_UnitAddress_ShouldBeAssignable()
        {
            var address = new UnitAddress
            {
                Id = 1,
                UnitId = 1,
                CountryId = 1,
                StateId = 2,
                CityId = 3,
                AddressLine1 = "123 Main Street",
                AddressLine2 = "Suite 100",
                PinCode = 123456,
                ContactNumber = "9876543210",
                AlternateNumber = "9876543211"
            };

            var entity = new DomainUnit
            {
                Id = 1,
                UnitAddress = address
            };

            entity.UnitAddress.Should().NotBeNull();
            entity.UnitAddress!.AddressLine1.Should().Be("123 Main Street");
            entity.UnitAddress.PinCode.Should().Be(123456);
        }

        [Fact]
        public void Unit_NavigationProperty_UnitContacts_ShouldBeAssignable()
        {
            var contacts = new UnitContacts
            {
                Id = 1,
                UnitId = 1,
                Name = "John Doe",
                Designation = "Manager",
                Email = "john@example.com",
                PhoneNo = "9876543210",
                Remarks = "Primary contact"
            };

            var entity = new DomainUnit
            {
                Id = 1,
                UnitContacts = contacts
            };

            entity.UnitContacts.Should().NotBeNull();
            entity.UnitContacts!.Name.Should().Be("John Doe");
            entity.UnitContacts.Email.Should().Be("john@example.com");
        }

        [Fact]
        public void UnitAddress_Properties_ShouldBeAssignable()
        {
            var address = new UnitAddress
            {
                Id = 5,
                UnitId = 1,
                CountryId = 1,
                StateId = 2,
                CityId = 3,
                AddressLine1 = "Address 1",
                AddressLine2 = "Address 2",
                PinCode = 654321,
                ContactNumber = "1234567890",
                AlternateNumber = "0987654321"
            };

            address.Id.Should().Be(5);
            address.UnitId.Should().Be(1);
            address.CountryId.Should().Be(1);
            address.StateId.Should().Be(2);
            address.CityId.Should().Be(3);
            address.PinCode.Should().Be(654321);
        }

        [Fact]
        public void UnitContacts_Properties_ShouldBeAssignable()
        {
            var contacts = new UnitContacts
            {
                Id = 5,
                UnitId = 1,
                Name = "Jane Doe",
                Designation = "Director",
                Email = "jane@example.com",
                PhoneNo = "1234567890",
                Remarks = "Some remark"
            };

            contacts.Id.Should().Be(5);
            contacts.UnitId.Should().Be(1);
            contacts.Name.Should().Be("Jane Doe");
            contacts.Designation.Should().Be("Director");
            contacts.Email.Should().Be("jane@example.com");
            contacts.PhoneNo.Should().Be("1234567890");
            contacts.Remarks.Should().Be("Some remark");
        }
    }
}
