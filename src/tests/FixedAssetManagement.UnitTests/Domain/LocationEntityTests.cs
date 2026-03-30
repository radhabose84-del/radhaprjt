using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class LocationEntityTests
    {
        [Fact]
        public void Location_DefaultIsActive_ShouldBeActive()
        {
            var entity = new Location();
            entity.IsActive.Should().Be(Status.Active);
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
                Code = "LOC001",
                LocationName = "Test Location",
                Description = "Test description",
                SortOrder = 2,
                UnitId = 3,
                DepartmentId = 4
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("LOC001");
            entity.LocationName.Should().Be("Test Location");
            entity.Description.Should().Be("Test description");
            entity.SortOrder.Should().Be(2);
            entity.UnitId.Should().Be(3);
            entity.DepartmentId.Should().Be(4);
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

        [Fact]
        public void Location_SubLocationsCollection_ShouldBeInitialized()
        {
            var entity = new Location();
            entity.SubLocations.Should().NotBeNull();
        }
    }
}
