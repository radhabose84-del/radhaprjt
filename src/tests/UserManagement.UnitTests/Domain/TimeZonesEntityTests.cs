using UserManagement.Domain.Common;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Domain
{
    public class TimeZonesEntityTests
    {
        [Fact]
        public void TimeZones_DefaultIsActive_ShouldBeInactive()
        {
            var entity = new TimeZones();
            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void TimeZones_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new TimeZones();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void TimeZones_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(TimeZones)).Should().BeTrue();
        }

        [Fact]
        public void TimeZones_Properties_ShouldBeAssignable()
        {
            var entity = new TimeZones
            {
                Id = 1,
                Code = "IST",
                Name = "Indian Standard Time",
                Location = "Asia/Kolkata",
                Offset = "UTC+05:30"
            };

            entity.Id.Should().Be(1);
            entity.Code.Should().Be("IST");
            entity.Name.Should().Be("Indian Standard Time");
            entity.Location.Should().Be("Asia/Kolkata");
            entity.Offset.Should().Be("UTC+05:30");
        }

        [Fact]
        public void TimeZones_NullableProperties_ShouldAcceptNull()
        {
            var entity = new TimeZones
            {
                Code = null,
                Name = null,
                Location = null,
                Offset = null
            };

            entity.Code.Should().BeNull();
            entity.Name.Should().BeNull();
            entity.Location.Should().BeNull();
            entity.Offset.Should().BeNull();
        }
    }
}
