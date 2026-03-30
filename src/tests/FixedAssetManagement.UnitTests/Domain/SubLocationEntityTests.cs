using FAM.Domain.Common;
using FAM.Domain.Entities;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Domain
{
    public class SubLocationEntityTests
    {
        [Fact]
        public void SubLocation_DefaultIsActive_ShouldBeActive()
        {
            var entity = new SubLocation();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void SubLocation_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new SubLocation();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void SubLocation_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(SubLocation)).Should().BeTrue();
        }

        [Fact]
        public void SubLocation_Properties_ShouldBeAssignable()
        {
            var entity = new SubLocation
            {
                Id = 1,
                Code = "SL001",
                SubLocationName = "Test SubLocation",
                Description = "Test description",
                UnitId = 2,
                DepartmentId = 3,
                LocationId = 4
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("SL001");
            entity.SubLocationName.Should().Be("Test SubLocation");
            entity.Description.Should().Be("Test description");
            entity.UnitId.Should().Be(2);
            entity.DepartmentId.Should().Be(3);
            entity.LocationId.Should().Be(4);
        }

        [Fact]
        public void SubLocation_NullableProperties_ShouldAcceptNull()
        {
            var entity = new SubLocation
            {
                Code = null,
                SubLocationName = null,
                Description = null,
                Location = null
            };

            entity.Code.Should().BeNull();
            entity.SubLocationName.Should().BeNull();
            entity.Description.Should().BeNull();
            entity.Location.Should().BeNull();
        }
    }
}
