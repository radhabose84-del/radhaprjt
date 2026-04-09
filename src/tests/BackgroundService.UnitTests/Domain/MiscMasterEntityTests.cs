using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class MiscMasterEntityTests
    {
        [Fact]
        public void MiscMaster_DefaultIsActive_ShouldBeActive()
        {
            var entity = new MiscMaster();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void MiscMaster_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new MiscMaster();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void MiscMaster_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(MiscMaster)).Should().BeTrue();
        }

        [Fact]
        public void MiscMaster_Properties_ShouldBeAssignable()
        {
            var entity = new MiscMaster
            {
                Id = 1,
                MiscTypeId = 10,
                Code = "MISC001",
                Description = "Test Misc Master",
                SortOrder = 5
            };

            entity.Id.Should().Be(1);
            entity.MiscTypeId.Should().Be(10);
            entity.Code.Should().Be("MISC001");
            entity.Description.Should().Be("Test Misc Master");
            entity.SortOrder.Should().Be(5);
        }

        [Fact]
        public void MiscMaster_NullableProperties_ShouldAcceptNull()
        {
            var entity = new MiscMaster
            {
                Code = null,
                Description = null
            };

            entity.Code.Should().BeNull();
            entity.Description.Should().BeNull();
        }

        [Fact]
        public void MiscMaster_NavigationProperties_ShouldBeAssignable()
        {
            var entity = new MiscMaster();

            entity.MiscType.Should().NotBeNull();
            entity.TargetType.Should().NotBeNull();
            entity.ApprovalMode.Should().NotBeNull();
            entity.Channels.Should().NotBeNull();
            entity.RecipientType.Should().NotBeNull();
            entity.NotificationEventType.Should().NotBeNull();
            entity.Channel.Should().NotBeNull();
            entity.NotificationStatus.Should().NotBeNull();
            entity.NotificationTemplates.Should().NotBeNull();
            entity.ReadStatus.Should().NotBeNull();
        }
    }
}
