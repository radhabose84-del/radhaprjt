using BackgroundService.Domain.Common;
using BackgroundService.Domain.Entities.Notification;
using static BackgroundService.Domain.Common.BaseEntity;

namespace BackgroundService.UnitTests.Domain
{
    public class NotificationEventLogEntityTests
    {
        [Fact]
        public void NotificationEventLog_DefaultIsActive_ShouldBeActive()
        {
            var entity = new NotificationEventLog();
            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void NotificationEventLog_DefaultIsDeleted_ShouldBeNotDeleted()
        {
            var entity = new NotificationEventLog();
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void NotificationEventLog_ShouldInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(NotificationEventLog)).Should().BeTrue();
        }

        [Fact]
        public void NotificationEventLog_Properties_ShouldBeAssignable()
        {
            var timestamp = DateTimeOffset.UtcNow;
            var entity = new NotificationEventLog
            {
                Id = 1,
                NotificationLevelRuleId = 5,
                UnitId = 10,
                ChannelId = 2,
                NotificationStatusId = 3,
                MessageText = "Test message",
                ActionStatus = "Sent",
                ReadStatusId = 1,
                Timestamp = timestamp,
                SendTo = "user@test.com",
                Value = "some-value",
                ModuleTransactionId = 42,
                ModuleTypeName = "Sales Quotation"
            };

            entity.Id.Should().Be(1);
            entity.NotificationLevelRuleId.Should().Be(5);
            entity.UnitId.Should().Be(10);
            entity.ChannelId.Should().Be(2);
            entity.NotificationStatusId.Should().Be(3);
            entity.MessageText.Should().Be("Test message");
            entity.ActionStatus.Should().Be("Sent");
            entity.ReadStatusId.Should().Be(1);
            entity.Timestamp.Should().Be(timestamp);
            entity.SendTo.Should().Be("user@test.com");
            entity.Value.Should().Be("some-value");
            entity.ModuleTransactionId.Should().Be(42);
            entity.ModuleTypeName.Should().Be("Sales Quotation");
        }

        [Fact]
        public void NotificationEventLog_NullableProperties_ShouldAcceptNull()
        {
            var entity = new NotificationEventLog
            {
                NotificationLevelRuleId = null,
                MessageText = null,
                ActionStatus = null,
                SendTo = null,
                Value = null,
                NotificationEventRules = null,
                Channel = null,
                NotificationStatus = null,
                ReadStatus = null,
                ModuleTypeName = null
            };

            entity.NotificationLevelRuleId.Should().BeNull();
            entity.MessageText.Should().BeNull();
            entity.ActionStatus.Should().BeNull();
            entity.SendTo.Should().BeNull();
            entity.Value.Should().BeNull();
            entity.NotificationEventRules.Should().BeNull();
            entity.Channel.Should().BeNull();
            entity.NotificationStatus.Should().BeNull();
            entity.ReadStatus.Should().BeNull();
            entity.ModuleTypeName.Should().BeNull();
        }

        [Fact]
        public void NotificationEventLog_ModuleTransactionId_DefaultShouldBeZero()
        {
            var entity = new NotificationEventLog();
            entity.ModuleTransactionId.Should().Be(0);
        }

        [Fact]
        public void NotificationEventLog_ModuleTypeName_DefaultShouldBeNull()
        {
            var entity = new NotificationEventLog();
            entity.ModuleTypeName.Should().BeNull();
        }
    }
}
