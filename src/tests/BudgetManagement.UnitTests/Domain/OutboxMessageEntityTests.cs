using BudgetManagement.Domain.Entities.Outbox;

namespace BudgetManagement.UnitTests.Domain
{
    public class OutboxMessageEntityTests
    {
        [Fact]
        public void OutboxMessage_ShouldNotInheritFromBaseEntity()
        {
            typeof(BudgetManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(OutboxMessage))
                .Should().BeFalse();
        }

        [Fact]
        public void OutboxMessage_DefaultStatus_ShouldBePending()
        {
            var message = new OutboxMessage();
            message.Status.Should().Be(OutboxMessageStatus.Pending);
        }

        [Fact]
        public void OutboxMessage_DefaultMaxRetries_ShouldBeFive()
        {
            var message = new OutboxMessage();
            message.MaxRetries.Should().Be(5);
        }

        [Fact]
        public void OutboxMessage_DefaultEventType_ShouldBeEmpty()
        {
            var message = new OutboxMessage();
            message.EventType.Should().BeEmpty();
        }

        [Fact]
        public void OutboxMessage_DefaultEventData_ShouldBeEmpty()
        {
            var message = new OutboxMessage();
            message.EventData.Should().BeEmpty();
        }

        [Fact]
        public void OutboxMessage_DefaultModuleName_ShouldBeBudgetManagement()
        {
            var message = new OutboxMessage();
            message.ModuleName.Should().Be("BudgetManagement");
        }

        [Fact]
        public void OutboxMessage_Properties_ShouldBeAssignable()
        {
            var correlationId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var message = new OutboxMessage
            {
                Id = 42,
                CorrelationId = correlationId,
                EventType = "BudgetCreated",
                EventData = "{\"id\":1}",
                Status = OutboxMessageStatus.Published,
                CreatedAt = now,
                PublishedAt = now.AddMinutes(1),
                RetryCount = 2,
                MaxRetries = 10,
                LastError = "Timeout",
                NextRetryAt = now.AddMinutes(5),
                ModuleName = "TestModule",
                CreatedBy = 7
            };

            message.Id.Should().Be(42);
            message.CorrelationId.Should().Be(correlationId);
            message.EventType.Should().Be("BudgetCreated");
            message.EventData.Should().Be("{\"id\":1}");
            message.Status.Should().Be(OutboxMessageStatus.Published);
            message.CreatedAt.Should().Be(now);
            message.PublishedAt.Should().Be(now.AddMinutes(1));
            message.RetryCount.Should().Be(2);
            message.MaxRetries.Should().Be(10);
            message.LastError.Should().Be("Timeout");
            message.NextRetryAt.Should().Be(now.AddMinutes(5));
            message.ModuleName.Should().Be("TestModule");
            message.CreatedBy.Should().Be(7);
        }

        [Fact]
        public void OutboxMessage_NullableProperties_ShouldAcceptNull()
        {
            var message = new OutboxMessage
            {
                PublishedAt = null,
                LastError = null,
                NextRetryAt = null,
                CreatedBy = null
            };

            message.PublishedAt.Should().BeNull();
            message.LastError.Should().BeNull();
            message.NextRetryAt.Should().BeNull();
            message.CreatedBy.Should().BeNull();
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
