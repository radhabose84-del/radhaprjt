using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class NotificationEventRuleEntityTests
    {
        [Fact]
        public void NotificationEventRule_DefaultIsActive_ShouldBeActive()
        {
            var entity = new NotificationEventRule();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void NotificationEventRule_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new NotificationEventRule();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void NotificationEventRule_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(NotificationEventRule)).Should().BeTrue();
        }

        [Fact]
        public void NotificationEventRule_Properties_ShouldBeAssignable()
        {
            var entity = new NotificationEventRule
            {
                Id = 1,
                NotificationLevelHierarchyId = 10,
                NotificationChannelId = 2,
                RecipientTypeId = 3,
                TemplateId = 4
            };

            entity.Id.Should().Be(1);
            entity.NotificationLevelHierarchyId.Should().Be(10);
            entity.NotificationChannelId.Should().Be(2);
            entity.RecipientTypeId.Should().Be(3);
            entity.TemplateId.Should().Be(4);
        }

        [Fact]
        public void NotificationEventRule_NullableNavigationProperties_ShouldAcceptNull()
        {
            var entity = new NotificationEventRule
            {
                RecipientType = null,
                Channel = null,
                NotificationTemplates = null,
                NotificationLevelHierarchy = null,
                NotificationEventLog = null
            };

            entity.RecipientType.Should().BeNull();
            entity.Channel.Should().BeNull();
            entity.NotificationTemplates.Should().BeNull();
            entity.NotificationLevelHierarchy.Should().BeNull();
            entity.NotificationEventLog.Should().BeNull();
        }
    }
}
