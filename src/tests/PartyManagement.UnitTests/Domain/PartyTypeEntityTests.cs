using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Domain
{
    public class PartyTypeEntityTests
    {
        [Fact]
        public void PartyType_ShouldNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(PartyType)).Should().BeFalse();
        }

        [Fact]
        public void PartyType_Properties_ShouldBeAssignable()
        {
            var entity = new PartyType
            {
                Id = 1,
                PartyId = 10,
                PartyTypeId = 3,
                PartyGroupId = 5
            };

            entity.Id.Should().Be(1);
            entity.PartyId.Should().Be(10);
            entity.PartyTypeId.Should().Be(3);
            entity.PartyGroupId.Should().Be(5);
        }

        [Fact]
        public void PartyType_NavigationProperties_ShouldBeAssignable()
        {
            var party = new PartyMaster { Id = 10 };
            var partyTypeMisc = new MiscMaster { Id = 3 };
            var partyGroup = new PartyGroup { Id = 5 };

            var entity = new PartyType
            {
                Party = party,
                PartyTypeMisc = partyTypeMisc,
                PartyGroup = partyGroup
            };

            entity.Party.Should().NotBeNull();
            entity.Party.Id.Should().Be(10);
            entity.PartyTypeMisc.Should().NotBeNull();
            entity.PartyGroup.Should().NotBeNull();
            entity.PartyGroup.Id.Should().Be(5);
        }
    }
}
