using ProjectManagement.Domain.Common;
using ProjectManagement.Domain.Entities.Outbox;

namespace ProjectManagement.UnitTests.Domain
{
    public class OutboxMessageEntityTests
    {
        [Fact]
        public void OutboxMessage_ShouldNotInheritFromBaseEntity()
        {
            typeof(BaseEntity).IsAssignableFrom(typeof(OutboxMessage)).Should().BeFalse();
        }

        [Fact]
        public void OutboxMessage_Properties_ShouldBeAssignable()
        {
            var correlationId = Guid.NewGuid();
            var createdAt = DateTimeOffset.UtcNow;
            var entity = new OutboxMessage
            {
                Id = 42,
                CorrelationId = correlationId,
                EventType = "ProjectCreated",
                EventData = "{\"projectId\":1}",
                Status = OutboxMessageStatus.Published,
                CreatedAt = createdAt,
                RetryCount = 2,
                MaxRetries = 10,
                LastError = "Timeout",
                ModuleName = "TestModule",
                CreatedBy = 5
            };

            entity.Id.Should().Be(42);
            entity.CorrelationId.Should().Be(correlationId);
            entity.EventType.Should().Be("ProjectCreated");
            entity.EventData.Should().Be("{\"projectId\":1}");
            entity.Status.Should().Be(OutboxMessageStatus.Published);
            entity.CreatedAt.Should().Be(createdAt);
            entity.RetryCount.Should().Be(2);
            entity.MaxRetries.Should().Be(10);
            entity.LastError.Should().Be("Timeout");
            entity.ModuleName.Should().Be("TestModule");
            entity.CreatedBy.Should().Be(5);
        }

        [Fact]
        public void OutboxMessage_DefaultValues_ShouldBeCorrect()
        {
            var entity = new OutboxMessage();

            entity.EventType.Should().Be(string.Empty);
            entity.EventData.Should().Be(string.Empty);
            entity.Status.Should().Be(OutboxMessageStatus.Pending);
            entity.MaxRetries.Should().Be(5);
            entity.ModuleName.Should().Be("ProjectManagement");
        }

        [Fact]
        public void OutboxMessage_NullableProperties_ShouldAcceptNull()
        {
            var entity = new OutboxMessage
            {
                PublishedAt = null,
                LastError = null,
                NextRetryAt = null,
                CreatedBy = null
            };

            entity.PublishedAt.Should().BeNull();
            entity.LastError.Should().BeNull();
            entity.NextRetryAt.Should().BeNull();
            entity.CreatedBy.Should().BeNull();
        }

        [Fact]
        public void OutboxMessage_NullableDateProperties_ShouldBeAssignable()
        {
            var publishedAt = DateTimeOffset.UtcNow;
            var nextRetryAt = DateTimeOffset.UtcNow.AddMinutes(5);
            var entity = new OutboxMessage
            {
                PublishedAt = publishedAt,
                NextRetryAt = nextRetryAt
            };

            entity.PublishedAt.Should().Be(publishedAt);
            entity.NextRetryAt.Should().Be(nextRetryAt);
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
