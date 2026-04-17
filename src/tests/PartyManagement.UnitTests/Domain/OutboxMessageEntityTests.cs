using PartyManagement.Domain.Entities.Outbox;

namespace PartyManagement.UnitTests.Domain
{
    public class OutboxMessageEntityTests
    {
        [Fact]
        public void OutboxMessage_ShouldNotInheritFromBaseEntity()
        {
            typeof(PartyManagement.Domain.Common.BaseEntity)
                .IsAssignableFrom(typeof(OutboxMessage)).Should().BeFalse();
        }

        [Fact]
        public void OutboxMessage_Properties_ShouldBeAssignable()
        {
            var correlationId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;

            var entity = new OutboxMessage
            {
                Id = 1,
                CorrelationId = correlationId,
                EventType = "PartyCreated",
                EventData = "{\"id\":1}",
                Status = OutboxMessageStatus.Pending,
                CreatedAt = now,
                PublishedAt = now.AddMinutes(1),
                RetryCount = 0,
                MaxRetries = 5,
                LastError = null,
                NextRetryAt = now.AddMinutes(5),
                ModuleName = "PartyManagement",
                CreatedBy = 1
            };

            entity.Id.Should().Be(1);
            entity.CorrelationId.Should().Be(correlationId);
            entity.EventType.Should().Be("PartyCreated");
            entity.EventData.Should().Be("{\"id\":1}");
            entity.Status.Should().Be(OutboxMessageStatus.Pending);
            entity.CreatedAt.Should().Be(now);
            entity.RetryCount.Should().Be(0);
            entity.MaxRetries.Should().Be(5);
            entity.ModuleName.Should().Be("PartyManagement");
            entity.CreatedBy.Should().Be(1);
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
        public void OutboxMessage_DefaultModuleName_ShouldBePartyManagement()
        {
            var entity = new OutboxMessage();
            entity.ModuleName.Should().Be("PartyManagement");
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
