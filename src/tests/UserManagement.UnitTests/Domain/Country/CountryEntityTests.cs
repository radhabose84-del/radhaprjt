using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain.Country
{
    public class CountryEntityTests
    {
        [Fact]
        public void Country_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new Countries();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Country_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new Countries();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Country_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Countries)).Should().BeTrue();
        }

        [Fact]
        public void Country_Properties_ShouldBeAssignable()
        {
            var entity = new Countries
            {
                Id = 1,
                CountryCode = "IND",
                CountryName = "India"
            };

            entity.Id.Should().Be(1);
            entity.CountryCode.Should().Be("IND");
            entity.CountryName.Should().Be("India");
        }

        [Fact]
        public void Country_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Countries
            {
                CountryCode = null,
                CountryName = null
            };

            entity.CountryCode.Should().BeNull();
            entity.CountryName.Should().BeNull();
        }

        [Fact]
        public void Country_NavigationProperty_States_ShouldDefaultToEmptyList()
        {
            var entity = new Countries();
            entity.States.Should().NotBeNull();
            entity.States.Should().BeEmpty();
        }

        [Fact]
        public void Country_NavigationProperty_States_ShouldBeAssignable()
        {
            var entity = new Countries
            {
                Id = 1,
                CountryCode = "IND",
                CountryName = "India",
                States = new List<States>
                {
                    new States { Id = 1, StateName = "Tamil Nadu", StateCode = "TN", CountryId = 1 }
                }
            };

            entity.States.Should().HaveCount(1);
            entity.States.First().StateName.Should().Be("Tamil Nadu");
        }
    }
}
