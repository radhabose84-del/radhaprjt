using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class CompanyEntityTests
    {
        [Fact]
        public void Company_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new Company();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Company_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new Company();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Company_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Company)).Should().BeTrue();
        }

        [Fact]
        public void Company_Properties_ShouldBeAssignable()
        {
            var entity = new Company
            {
                Id = 1,
                CompanyName = "Test Co",
                LegalName = "Test Co Pvt Ltd",
                GstNumber = "22AAAAA0000A1Z5",
                TIN = "TIN001",
                TAN = "TAN001",
                CSTNo = "CST001",
                YearOfEstablishment = 2010,
                Website = "https://test.com",
                Logo = "logo.png",
                EntityId = 5,
                PanNumber = "ABCDE1234F"
            };

            entity.Id.Should().Be(1);
            entity.CompanyName.Should().Be("Test Co");
            entity.LegalName.Should().Be("Test Co Pvt Ltd");
            entity.GstNumber.Should().Be("22AAAAA0000A1Z5");
            entity.YearOfEstablishment.Should().Be(2010);
            entity.EntityId.Should().Be(5);
            entity.PanNumber.Should().Be("ABCDE1234F");
        }

        [Fact]
        public void Company_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Company
            {
                CompanyName = null,
                LegalName = null,
                GstNumber = null,
                TIN = null,
                TAN = null,
                CSTNo = null,
                Website = null,
                Logo = null,
                PanNumber = null,
                CompanyAddress = null,
                CompanyContact = null,
                UserCompanies = null,
                CompanySettings = null,
                Units = null,
                Divisions = null
            };

            entity.CompanyName.Should().BeNull();
            entity.GstNumber.Should().BeNull();
            entity.PanNumber.Should().BeNull();
            entity.CompanyAddress.Should().BeNull();
            entity.Units.Should().BeNull();
            entity.Divisions.Should().BeNull();
        }

        [Fact]
        public void Company_NavigationProperty_Units_ShouldBeAssignable()
        {
            var units = new List<Unit>
            {
                new Unit { Id = 1 },
                new Unit { Id = 2 }
            };

            var entity = new Company { Units = units };

            entity.Units.Should().NotBeNull();
            entity.Units.Should().HaveCount(2);
        }

        [Fact]
        public void Company_NavigationProperty_Divisions_ShouldBeAssignable()
        {
            var divisions = new List<UserManagement.Domain.Entities.Division>
            {
                new UserManagement.Domain.Entities.Division { Id = 1 },
                new UserManagement.Domain.Entities.Division { Id = 2 },
                new UserManagement.Domain.Entities.Division { Id = 3 }
            };

            var entity = new Company { Divisions = divisions };

            entity.Divisions.Should().HaveCount(3);
        }
    }
}
