using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class NotificationConfigEntityTests
    {
        [Fact]
        public void NotificationConfig_DefaultIsActive_ShouldBeActive()
        {
            var entity = new NotificationConfig();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void NotificationConfig_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new NotificationConfig();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void NotificationConfig_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(NotificationConfig)).Should().BeTrue();
        }

        [Fact]
        public void NotificationConfig_Properties_ShouldBeAssignable()
        {
            var entity = new NotificationConfig
            {
                Id = 1,
                ModuleName = "Sales",
                NotificationEventTypeId = 5,
                UnitId = 10
            };

            entity.Id.Should().Be(1);
            entity.ModuleName.Should().Be("Sales");
            entity.NotificationEventTypeId.Should().Be(5);
            entity.UnitId.Should().Be(10);
        }

        [Fact]
        public void NotificationConfig_NullableProperties_ShouldAcceptNull()
        {
            var entity = new NotificationConfig
            {
                ModuleName = null,
                NotificationEventType = null
            };

            entity.ModuleName.Should().BeNull();
            entity.NotificationEventType.Should().BeNull();
        }

        [Fact]
        public void NotificationConfig_NavigationCollections_ShouldBeInitialized()
        {
            var entity = new NotificationConfig();

            entity.NotificationLevelHierarchies.Should().NotBeNull();
            entity.NotificationTemplates.Should().NotBeNull();
        }
    }
}
