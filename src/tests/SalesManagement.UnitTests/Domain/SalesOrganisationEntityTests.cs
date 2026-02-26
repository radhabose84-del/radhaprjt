using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class SalesOrganisationEntityTests
    {
        [Fact]
        public void SalesOrganisation_DefaultIsActive_ShouldBeActive()
        {
            var entity = new SalesOrganisation();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void SalesOrganisation_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new SalesOrganisation();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void SalesOrganisation_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SalesOrganisation)).Should().BeTrue();
        }

        [Fact]
        public void SalesOrganisation_Properties_ShouldBeAssignable()
        {
            var entity = new SalesOrganisation
            {
                Id = 1,
                SalesOrganisationCode = "ORG001",
                SalesOrganisationName = "Test Org",
                CompanyId = 10,
                Description = "Desc"
            };

            entity.Id.Should().Be(1);
            entity.SalesOrganisationCode.Should().Be("ORG001");
            entity.SalesOrganisationName.Should().Be("Test Org");
            entity.CompanyId.Should().Be(10);
            entity.Description.Should().Be("Desc");
        }

        [Fact]
        public void SalesOrganisation_Collections_ShouldBeAssignable()
        {
            var entity = new SalesOrganisation
            {
                SalesSegments = new List<SalesSegment>(),
                SalesOffices = new List<SalesOffice>()
            };

            entity.SalesSegments.Should().NotBeNull();
            entity.SalesOffices.Should().NotBeNull();
        }
    }
}
