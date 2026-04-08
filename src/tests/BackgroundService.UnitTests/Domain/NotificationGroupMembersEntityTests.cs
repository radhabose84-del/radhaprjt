using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class NotificationGroupMembersEntityTests
    {
        [Fact]
        public void NotificationGroupMembers_DefaultIsActive_ShouldBeActive()
        {
            var entity = new NotificationGroupMembers();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void NotificationGroupMembers_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new NotificationGroupMembers();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void NotificationGroupMembers_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(NotificationGroupMembers)).Should().BeTrue();
        }

        [Fact]
        public void NotificationGroupMembers_Properties_ShouldBeAssignable()
        {
            var group = new NotificationGroup { Id = 10, GroupName = "Test Group" };
            var entity = new NotificationGroupMembers
            {
                Id = 1,
                GroupId = 10,
                UserId = 25,
                Group = group
            };

            entity.Id.Should().Be(1);
            entity.GroupId.Should().Be(10);
            entity.UserId.Should().Be(25);
            entity.Group.Should().Be(group);
        }
    }
}
