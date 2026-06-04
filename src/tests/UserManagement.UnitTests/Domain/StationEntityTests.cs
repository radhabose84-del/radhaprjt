using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class StationEntityTests
    {
        [Fact]
        public void Station_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new Station();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Station_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new Station();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Station_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Station)).Should().BeTrue();
        }

        [Fact]
        public void Station_Properties_ShouldBeAssignable()
        {
            var entity = new Station
            {
                Id = 1,
                Code = "STA-0001",
                StationName = "Central Station",
                Description = "Primary station"
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("STA-0001");
            entity.StationName.Should().Be("Central Station");
            entity.Description.Should().Be("Primary station");
        }

        [Fact]
        public void Station_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Station
            {
                Code = null,
                StationName = null,
                Description = null
            };

            entity.Code.Should().BeNull();
            entity.StationName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
