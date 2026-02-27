using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Domain
{
    public class SalesOfficeEntityTests
    {
        [Fact]
        public void SalesOffice_DefaultIsActive_ShouldBeActive()
        {
            var entity = new SalesOffice();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void SalesOffice_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new SalesOffice();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void SalesOffice_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SalesOffice)).Should().BeTrue();
        }

        [Fact]
        public void SalesOffice_Properties_ShouldBeAssignable()
        {
            var entity = new SalesOffice
            {
                Id = 9,
                SalesOfficeName = "Mumbai Office",
                SalesOrganisationId = 1,
                CityId = 42,
                Pincode = "400001",
                Phone = "022-12345678",
                Email = "mumbai@sales.com",
                ResponsibleManager = "Jane Smith",
                RegionTerritory = "West",
                Address = "123 Business Park, Mumbai"
            };

            entity.Id.Should().Be(9);
            entity.SalesOfficeName.Should().Be("Mumbai Office");
            entity.SalesOrganisationId.Should().Be(1);
            entity.CityId.Should().Be(42);
            entity.Pincode.Should().Be("400001");
            entity.Phone.Should().Be("022-12345678");
            entity.Email.Should().Be("mumbai@sales.com");
            entity.ResponsibleManager.Should().Be("Jane Smith");
            entity.RegionTerritory.Should().Be("West");
            entity.Address.Should().Be("123 Business Park, Mumbai");
        }

        [Fact]
        public void SalesOffice_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new SalesOffice
            {
                SalesOrganisation = new SalesOrganisation(),
                SalesGroups = new List<SalesGroup>()
            };

            entity.SalesOrganisation.Should().NotBeNull();
            entity.SalesGroups.Should().NotBeNull();
        }
    }
}
