using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Outbox;

namespace PurchaseManagement.UnitTests.Domain
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
            var now = DateTimeOffset.UtcNow;
            var correlationId = Guid.NewGuid();
            var entity = new OutboxMessage
            {
                Id = 1,
                CorrelationId = correlationId,
                EventType = "PurchaseManagement.Domain.Events.OrderCreated",
                EventData = "{\"orderId\":1}",
                Status = OutboxMessageStatus.Published,
                CreatedAt = now,
                PublishedAt = now.AddSeconds(5),
                RetryCount = 1,
                MaxRetries = 5,
                LastError = null,
                NextRetryAt = now.AddMinutes(1),
                ModuleName = "PurchaseManagement",
                CreatedBy = 1
            };

            entity.Id.Should().Be(1);
            entity.CorrelationId.Should().Be(correlationId);
            entity.EventType.Should().Contain("OrderCreated");
            entity.EventData.Should().Contain("orderId");
            entity.Status.Should().Be(OutboxMessageStatus.Published);
            entity.CreatedAt.Should().Be(now);
            entity.PublishedAt.Should().Be(now.AddSeconds(5));
            entity.RetryCount.Should().Be(1);
            entity.MaxRetries.Should().Be(5);
            entity.ModuleName.Should().Be("PurchaseManagement");
            entity.CreatedBy.Should().Be(1);
        }

        [Fact]
        public void OutboxMessage_DefaultValues_ShouldBeCorrect()
        {
            var entity = new OutboxMessage();

            entity.Status.Should().Be(OutboxMessageStatus.Pending);
            entity.MaxRetries.Should().Be(5);
            entity.ModuleName.Should().Be("PurchaseManagement");
            entity.EventType.Should().Be(string.Empty);
            entity.EventData.Should().Be(string.Empty);
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
        public void OutboxMessageStatus_ShouldHaveCorrectValues()
        {
            ((int)OutboxMessageStatus.Pending).Should().Be(0);
            ((int)OutboxMessageStatus.Published).Should().Be(1);
            ((int)OutboxMessageStatus.Failed).Should().Be(2);
        }
    }
}
