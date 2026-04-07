using BackgroundService.Domain.Common;
using BackgroundService.Core.Domain.Entities.Notifications;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class NotificationWhatsAppGroupEntityTests
    {
        [Fact]
        public void NotificationWhatsAppGroup_DefaultIsActive_ShouldBeActive()
        {
            var entity = new NotificationWhatsAppGroup();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void NotificationWhatsAppGroup_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new NotificationWhatsAppGroup();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void NotificationWhatsAppGroup_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(NotificationWhatsAppGroup)).Should().BeTrue();
        }

        [Fact]
        public void NotificationWhatsAppGroup_Properties_ShouldBeAssignable()
        {
            var entity = new NotificationWhatsAppGroup
            {
                Id = 1,
                UnitId = 5,
                DepartmentId = 10,
                GroupName = "Test WhatsApp Group",
                ApiKey = "api-key-123"
            };

            entity.Id.Should().Be(1);
            entity.UnitId.Should().Be(5);
            entity.DepartmentId.Should().Be(10);
            entity.GroupName.Should().Be("Test WhatsApp Group");
            entity.ApiKey.Should().Be("api-key-123");
        }

        [Fact]
        public void NotificationWhatsAppGroup_NullableProperties_ShouldAcceptNull()
        {
            var entity = new NotificationWhatsAppGroup
            {
                ApiKey = null
            };

            entity.ApiKey.Should().BeNull();
        }
    }
}
