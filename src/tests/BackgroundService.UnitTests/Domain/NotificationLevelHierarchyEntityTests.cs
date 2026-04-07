using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class NotificationLevelHierarchyEntityTests
    {
        [Fact]
        public void NotificationLevelHierarchy_DefaultIsActive_ShouldBeActive()
        {
            var entity = new NotificationLevelHierarchy();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void NotificationLevelHierarchy_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new NotificationLevelHierarchy();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void NotificationLevelHierarchy_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(NotificationLevelHierarchy)).Should().BeTrue();
        }

        [Fact]
        public void NotificationLevelHierarchy_Properties_ShouldBeAssignable()
        {
            var entity = new NotificationLevelHierarchy
            {
                Id = 1,
                NotificationConfigId = 5,
                TargetTypeId = 2,
                TargetId = 10,
                ApprovalModeId = 3,
                Description = "Test Hierarchy"
            };

            entity.Id.Should().Be(1);
            entity.NotificationConfigId.Should().Be(5);
            entity.TargetTypeId.Should().Be(2);
            entity.TargetId.Should().Be(10);
            entity.ApprovalModeId.Should().Be(3);
            entity.Description.Should().Be("Test Hierarchy");
        }

        [Fact]
        public void NotificationLevelHierarchy_NullableProperties_ShouldAcceptNull()
        {
            var entity = new NotificationLevelHierarchy
            {
                Description = null,
                NotificationConfig = null,
                TargetType = null,
                ApprovalMode = null,
                NotificationEventRules = null
            };

            entity.Description.Should().BeNull();
            entity.NotificationConfig.Should().BeNull();
            entity.TargetType.Should().BeNull();
            entity.ApprovalMode.Should().BeNull();
            entity.NotificationEventRules.Should().BeNull();
        }
    }
}
