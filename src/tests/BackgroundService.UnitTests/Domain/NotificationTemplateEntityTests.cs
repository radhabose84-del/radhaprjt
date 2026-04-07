using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class NotificationTemplateEntityTests
    {
        [Fact]
        public void NotificationTemplate_DefaultIsActive_ShouldBeActive()
        {
            var entity = new NotificationTemplate();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void NotificationTemplate_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new NotificationTemplate();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void NotificationTemplate_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(NotificationTemplate)).Should().BeTrue();
        }

        [Fact]
        public void NotificationTemplate_Properties_ShouldBeAssignable()
        {
            var entity = new NotificationTemplate
            {
                Id = 1,
                NotificationTypeId = 2,
                NotificationConfigId = 3,
                SubjectTemplate = "Subject: {{name}}",
                HeaderTemplate = "Header content",
                BodyTemplate = "Body: {{details}}",
                LanguageCode = "en",
                FooterTemplate = "Footer content",
                IsTable = true
            };

            entity.Id.Should().Be(1);
            entity.NotificationTypeId.Should().Be(2);
            entity.NotificationConfigId.Should().Be(3);
            entity.SubjectTemplate.Should().Be("Subject: {{name}}");
            entity.HeaderTemplate.Should().Be("Header content");
            entity.BodyTemplate.Should().Be("Body: {{details}}");
            entity.LanguageCode.Should().Be("en");
            entity.FooterTemplate.Should().Be("Footer content");
            entity.IsTable.Should().BeTrue();
        }

        [Fact]
        public void NotificationTemplate_NullableProperties_ShouldAcceptNull()
        {
            var entity = new NotificationTemplate
            {
                SubjectTemplate = null,
                HeaderTemplate = null,
                BodyTemplate = null,
                LanguageCode = null,
                FooterTemplate = null,
                NotificationType = null
            };

            entity.SubjectTemplate.Should().BeNull();
            entity.HeaderTemplate.Should().BeNull();
            entity.BodyTemplate.Should().BeNull();
            entity.LanguageCode.Should().BeNull();
            entity.FooterTemplate.Should().BeNull();
            entity.NotificationType.Should().BeNull();
        }

        [Fact]
        public void NotificationTemplate_NavigationCollections_ShouldBeInitialized()
        {
            var entity = new NotificationTemplate();

            entity.NotificationEventRules.Should().NotBeNull();
        }
    }
}
