using SalesManagement.Domain.Entities.Outbox;

namespace SalesManagement.UnitTests.Domain;

public class OutboxMessageEntityTests
{
    [Fact]
    public void ShouldNotInheritFromBaseEntity()
    {
        typeof(SalesManagement.Domain.Common.BaseEntity)
            .IsAssignableFrom(typeof(OutboxMessage)).Should().BeFalse();
    }

    [Fact]
    public void DefaultStatus_ShouldBePending()
    {
        var entity = new OutboxMessage();
        entity.Status.Should().Be(OutboxMessageStatus.Pending);
    }

    [Fact]
    public void DefaultMaxRetries_ShouldBeFive()
    {
        var entity = new OutboxMessage();
        entity.MaxRetries.Should().Be(5);
    }

    [Fact]
    public void DefaultModuleName_ShouldBeSalesManagement()
    {
        var entity = new OutboxMessage();
        entity.ModuleName.Should().Be("SalesManagement");
    }

    [Fact]
    public void DefaultEventType_ShouldBeEmpty()
    {
        var entity = new OutboxMessage();
        entity.EventType.Should().BeEmpty();
    }

    [Fact]
    public void DefaultEventData_ShouldBeEmpty()
    {
        var entity = new OutboxMessage();
        entity.EventData.Should().BeEmpty();
    }

    [Fact]
    public void Properties_ShouldBeAssignable()
    {
        var now = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid();

        var entity = new OutboxMessage
        {
            Id = 42,
            CorrelationId = correlationId,
            EventType = "OrderCreated",
            EventData = "{\"orderId\":1}",
            Status = OutboxMessageStatus.Published,
            CreatedAt = now,
            PublishedAt = now.AddSeconds(5),
            RetryCount = 2,
            MaxRetries = 10,
            LastError = "Timeout",
            NextRetryAt = now.AddMinutes(1),
            ModuleName = "CustomModule",
            CreatedBy = 7
        };

        entity.Id.Should().Be(42);
        entity.CorrelationId.Should().Be(correlationId);
        entity.EventType.Should().Be("OrderCreated");
        entity.EventData.Should().Be("{\"orderId\":1}");
        entity.Status.Should().Be(OutboxMessageStatus.Published);
        entity.CreatedAt.Should().Be(now);
        entity.PublishedAt.Should().Be(now.AddSeconds(5));
        entity.RetryCount.Should().Be(2);
        entity.MaxRetries.Should().Be(10);
        entity.LastError.Should().Be("Timeout");
        entity.NextRetryAt.Should().Be(now.AddMinutes(1));
        entity.ModuleName.Should().Be("CustomModule");
        entity.CreatedBy.Should().Be(7);
    }

    [Fact]
    public void NullableProperties_ShouldAcceptNull()
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
    public void OutboxMessageStatus_ShouldHaveExpectedValues()
    {
        ((int)OutboxMessageStatus.Pending).Should().Be(0);
        ((int)OutboxMessageStatus.Published).Should().Be(1);
        ((int)OutboxMessageStatus.Failed).Should().Be(2);
    }
}
