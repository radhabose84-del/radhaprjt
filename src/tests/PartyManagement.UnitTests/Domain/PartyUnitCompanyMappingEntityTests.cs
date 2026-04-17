using PartyManagement.Domain.Entities;

namespace PartyManagement.UnitTests.Domain
{
    public class PartyUnitCompanyMappingEntityTests
    {
        [Fact]
        public void PartyUnitCompanyMapping_ShouldNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(PartyUnitCompanyMapping)).Should().BeFalse();
        }

        [Fact]
        public void PartyUnitCompanyMapping_Properties_ShouldBeAssignable()
        {
            var entity = new PartyUnitCompanyMapping
            {
                Id = 1,
                PartyId = 10,
                CompanyId = 5,
                UnitId = 3
            };

            entity.Id.Should().Be(1);
            entity.PartyId.Should().Be(10);
            entity.CompanyId.Should().Be(5);
            entity.UnitId.Should().Be(3);
        }

        [Fact]
        public void PartyUnitCompanyMapping_NavigationProperty_ShouldBeAssignable()
        {
            var party = new PartyMaster { Id = 10, PartyCode = "P001" };
            var entity = new PartyUnitCompanyMapping
            {
                PartyUnitCompany = party
            };

            entity.PartyUnitCompany.Should().NotBeNull();
            entity.PartyUnitCompany.Id.Should().Be(10);
        }
    }
}
