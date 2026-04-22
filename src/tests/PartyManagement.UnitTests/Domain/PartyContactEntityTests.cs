using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Domain
{
    public class PartyContactEntityTests
    {
        [Fact]
        public void PartyContact_ShouldNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(PartyContact)).Should().BeFalse();
        }

        [Fact]
        public void PartyContact_Properties_ShouldBeAssignable()
        {
            var entity = new PartyContact
            {
                Id = 1,
                PartyId = 10,
                FirstName = "John",
                LastName = "Doe",
                GenderId = 1,
                Designation = "Manager",
                EmailID = "john@example.com",
                MobileNo = "9876543210",
                Phone = "04412345678",
                PreferredChannelId = 2,
                ContactTypeId = 3,
                ContactBy = "Email"
            };

            entity.Id.Should().Be(1);
            entity.PartyId.Should().Be(10);
            entity.FirstName.Should().Be("John");
            entity.LastName.Should().Be("Doe");
            entity.GenderId.Should().Be(1);
            entity.Designation.Should().Be("Manager");
            entity.EmailID.Should().Be("john@example.com");
            entity.MobileNo.Should().Be("9876543210");
            entity.Phone.Should().Be("04412345678");
            entity.PreferredChannelId.Should().Be(2);
            entity.ContactTypeId.Should().Be(3);
            entity.ContactBy.Should().Be("Email");
        }

        [Fact]
        public void PartyContact_NullableProperties_ShouldAcceptNull()
        {
            var entity = new PartyContact
            {
                FirstName = null,
                LastName = null,
                GenderId = null,
                Designation = null,
                EmailID = null,
                MobileNo = null,
                Phone = null,
                PreferredChannelId = null,
                ContactTypeId = null,
                ContactBy = null
            };

            entity.FirstName.Should().BeNull();
            entity.LastName.Should().BeNull();
            entity.GenderId.Should().BeNull();
            entity.Designation.Should().BeNull();
            entity.EmailID.Should().BeNull();
            entity.MobileNo.Should().BeNull();
            entity.PreferredChannelId.Should().BeNull();
            entity.ContactTypeId.Should().BeNull();
            entity.ContactBy.Should().BeNull();
        }

        [Fact]
        public void PartyContact_NavigationProperties_ShouldBeAssignable()
        {
            var party = new PartyMaster { Id = 10 };
            var gender = new MiscMaster { Id = 1 };
            var channel = new MiscMaster { Id = 2 };
            var contactType = new MiscMaster { Id = 3 };

            var entity = new PartyContact
            {
                PartyContactId = party,
                Gender = gender,
                PreferredChannel = channel,
                ContactType = contactType
            };

            entity.PartyContactId.Should().NotBeNull();
            entity.Gender.Should().NotBeNull();
            entity.PreferredChannel.Should().NotBeNull();
            entity.ContactType.Should().NotBeNull();
        }
    }
}
