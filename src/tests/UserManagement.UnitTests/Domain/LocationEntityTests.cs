using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class LocationEntityTests
    {
        [Fact]
        public void Location_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new Location();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void Location_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new Location();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void Location_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(Location)).Should().BeTrue();
        }

        [Fact]
        public void Location_Properties_ShouldBeAssignable()
        {
            var entity = new Location
            {
                Id = 1,
                Code = "LOC-0001",
                LocationName = "Main Warehouse",
                Description = "Primary storage location"
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("LOC-0001");
            entity.LocationName.Should().Be("Main Warehouse");
            entity.Description.Should().Be("Primary storage location");
        }

        [Fact]
        public void Location_NullableProperties_ShouldAcceptNull()
        {
            var entity = new Location
            {
                Code = null,
                LocationName = null,
                Description = null
            };

            entity.Code.Should().BeNull();
            entity.LocationName.Should().BeNull();
            entity.Description.Should().BeNull();
        }
    }
}
