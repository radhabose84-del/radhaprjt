using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class NotificationGroupEntityTests
    {
        [Fact]
        public void NotificationGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new NotificationGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void NotificationGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new NotificationGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void NotificationGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(NotificationGroup)).Should().BeTrue();
        }

        [Fact]
        public void NotificationGroup_Properties_ShouldBeAssignable()
        {
            var entity = new NotificationGroup
            {
                Id = 1,
                GroupName = "Admin Group",
                UnitId = 5
            };

            entity.Id.Should().Be(1);
            entity.GroupName.Should().Be("Admin Group");
            entity.UnitId.Should().Be(5);
        }

        [Fact]
        public void NotificationGroup_NullableProperties_ShouldAcceptNull()
        {
            var entity = new NotificationGroup
            {
                GroupName = null
            };

            entity.GroupName.Should().BeNull();
        }
    }
}
