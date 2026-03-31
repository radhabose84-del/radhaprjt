using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain.City
{
    public class CityEntityTests
    {
        [Fact]
        public void City_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new Cities();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void City_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new Cities();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void City_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Cities)).Should().BeTrue();
        }

        [Fact]
        public void City_Properties_ShouldBeAssignable()
        {
            var entity = new Cities
            {
                Id = 1,
                CityCode = "CTY01",
                CityName = "Test City",
                StateId = 5
            };

            entity.Id.Should().Be(1);
            entity.CityCode.Should().Be("CTY01");
            entity.CityName.Should().Be("Test City");
            entity.StateId.Should().Be(5);
        }

        [Fact]
        public void City_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Cities
            {
                CityCode = null,
                CityName = null,
                States = null
            };

            entity.CityCode.Should().BeNull();
            entity.CityName.Should().BeNull();
            entity.States.Should().BeNull();
        }

        [Fact]
        public void City_NavigationProperty_States_ShouldBeAssignable()
        {
            var state = new States
            {
                Id = 1,
                StateName = "Tamil Nadu",
                StateCode = "TN",
                CountryId = 1
            };

            var entity = new Cities
            {
                Id = 1,
                StateId = 1,
                States = state
            };

            entity.States.Should().NotBeNull();
            entity.States!.StateName.Should().Be("Tamil Nadu");
        }
    }
}
