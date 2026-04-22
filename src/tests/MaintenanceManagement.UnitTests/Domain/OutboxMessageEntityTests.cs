using MaintenanceManagement.Domain.Entities.Outbox;

namespace MaintenanceManagement.UnitTests.Domain
{
    public class OutboxMessageEntityTests
    {
        [Fact]
        public void OutboxMessage_DoesNotInheritFromBaseEntity()
        {
            typeof(MaintenanceManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(OutboxMessage)).Should().BeFalse();
        }

        [Fact]
        public void OutboxMessage_DefaultStatus_ShouldBePending()
        {
            var entity = new OutboxMessage();
            entity.Status.Should().Be(OutboxMessageStatus.Pending);
        }

        [Fact]
        public void OutboxMessage_DefaultMaxRetries_ShouldBeFive()
        {
            var entity = new OutboxMessage();
            entity.MaxRetries.Should().Be(5);
        }

        [Fact]
        public void OutboxMessage_DefaultModuleName_ShouldBeMaintenanceManagement()
        {
            var entity = new OutboxMessage();
            entity.ModuleName.Should().Be("MaintenanceManagement");
        }

        [Fact]
        public void OutboxMessage_Properties_ShouldBeAssignable()
        {
            var now = DateTimeOffset.UtcNow;
            var correlationId = Guid.NewGuid();
            var entity = new OutboxMessage
            {
                Id = 100,
                CorrelationId = correlationId,
                EventType = "OrderCreated",
                EventData = "{\"id\":1}",
                Status = OutboxMessageStatus.Published,
                CreatedAt = now,
                PublishedAt = now.AddSeconds(5),
                RetryCount = 2,
                MaxRetries = 10,
                LastError = "Timeout",
                NextRetryAt = now.AddMinutes(1),
                ModuleName = "TestModule",
                CreatedBy = 5,
                ProcessorHint = "maintenance"
            };
            entity.Id.Should().Be(100);
            entity.CorrelationId.Should().Be(correlationId);
            entity.EventType.Should().Be("OrderCreated");
            entity.EventData.Should().Be("{\"id\":1}");
            entity.Status.Should().Be(OutboxMessageStatus.Published);
            entity.CreatedAt.Should().Be(now);
            entity.PublishedAt.Should().Be(now.AddSeconds(5));
            entity.RetryCount.Should().Be(2);
            entity.MaxRetries.Should().Be(10);
            entity.LastError.Should().Be("Timeout");
            entity.NextRetryAt.Should().Be(now.AddMinutes(1));
            entity.ModuleName.Should().Be("TestModule");
            entity.CreatedBy.Should().Be(5);
            entity.ProcessorHint.Should().Be("maintenance");
        }

        [Fact]
        public void OutboxMessage_NullableProperties_ShouldAcceptNull()
        {
            var entity = new OutboxMessage
            {
                PublishedAt = null,
                LastError = null,
                NextRetryAt = null,
                CreatedBy = null,
                ProcessorHint = null
            };
            entity.PublishedAt.Should().BeNull();
            entity.LastError.Should().BeNull();
            entity.NextRetryAt.Should().BeNull();
            entity.CreatedBy.Should().BeNull();
            entity.ProcessorHint.Should().BeNull();
        }

        [Fact]
        public void OutboxMessageStatus_ShouldHaveCorrectValues()
        {
            ((int)OutboxMessageStatus.Pending).Should().Be(0);
            ((int)OutboxMessageStatus.Published).Should().Be(1);
            ((int)OutboxMessageStatus.Failed).Should().Be(2);
        }
    }
}
