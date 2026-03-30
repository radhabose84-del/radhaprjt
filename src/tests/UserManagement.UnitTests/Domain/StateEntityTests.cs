using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class StateEntityTests
    {
        [Fact]
        public void State_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new States();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void State_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new States();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void State_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(States)).Should().BeTrue();
        }

        [Fact]
        public void State_Properties_ShouldBeAssignable()
        {
            var entity = new States
            {
                Id = 1,
                StateCode = "MH",
                StateName = "Maharashtra",
                CountryId = 10
            };

            entity.Id.Should().Be(1);
            entity.StateCode.Should().Be("MH");
            entity.StateName.Should().Be("Maharashtra");
            entity.CountryId.Should().Be(10);
        }

        [Fact]
        public void State_NullableProperties_ShouldAcceptNull()
        {
            var entity = new States
            {
                StateCode = null,
                StateName = null
            };

            entity.StateCode.Should().BeNull();
            entity.StateName.Should().BeNull();
        }

        [Fact]
        public void State_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new States
            {
                Countries = new Countries(),
                Cities = new List<Cities>()
            };

            entity.Countries.Should().NotBeNull();
            entity.Cities.Should().NotBeNull();
            entity.Cities.Should().BeEmpty();
        }

        [Fact]
        public void State_Cities_Collection_DefaultsToEmptyList()
        {
            var entity = new States();
            entity.Cities.Should().NotBeNull();
            entity.Cities.Should().BeEmpty();
        }
    }
}
