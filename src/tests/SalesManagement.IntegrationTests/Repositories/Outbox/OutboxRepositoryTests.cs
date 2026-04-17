using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesManagement.Domain.Entities.Outbox;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.Outbox;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Outbox
{
    /// <summary>
    /// Integration tests for OutboxRepository.
    /// OutboxMessage is a standalone entity (no FK dependencies on Sales tables).
    /// Tests verify Add, GetPending, MarkAsPublished, MarkAsFailed, DeleteOld, GetByCorrelationId.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class OutboxRepositoryTests
    {
        private readonly DbFixture _fixture;

        public OutboxRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private OutboxRepository CreateRepository(ApplicationDbContext ctx)
            => new OutboxRepository(ctx, new Mock<ILogger<OutboxRepository>>(MockBehavior.Loose).Object);

        private static OutboxMessage BuildMessage(
            Guid? correlationId = null,
            string eventType = "TestEvent",
            string eventData = "{\"test\":true}",
            OutboxMessageStatus status = OutboxMessageStatus.Pending,
            int maxRetries = 5)
            => new OutboxMessage
            {
                CorrelationId = correlationId ?? Guid.NewGuid(),
                EventType = eventType,
                EventData = eventData,
                Status = status,
                CreatedAt = DateTimeOffset.UtcNow,
                MaxRetries = maxRetries,
                ModuleName = "SalesManagement",
                CreatedBy = 1
            };

        private async Task ClearOutboxAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.OutboxMessages");
        }

        // ---------------------------------------------------------------------------
        // AddAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task AddAsync_Should_Persist_Message()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage();
            await CreateRepository(ctx).AddAsync(msg);

            msg.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddAsync_Should_Persist_Fields_Correctly()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var correlationId = Guid.NewGuid();
            var msg = BuildMessage(correlationId, "MyEvent", "{\"key\":1}");
            await CreateRepository(ctx).AddAsync(msg);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);

            saved.Should().NotBeNull();
            saved!.CorrelationId.Should().Be(correlationId);
            saved.EventType.Should().Be("MyEvent");
            saved.EventData.Should().Be("{\"key\":1}");
            saved.Status.Should().Be(OutboxMessageStatus.Pending);
            saved.ModuleName.Should().Be("SalesManagement");
            saved.RetryCount.Should().Be(0);
        }

        // ---------------------------------------------------------------------------
        // GetPendingMessagesAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Return_Pending_Only()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            await repo.AddAsync(BuildMessage(status: OutboxMessageStatus.Pending));
            await repo.AddAsync(BuildMessage(status: OutboxMessageStatus.Published));

            var pending = await repo.GetPendingMessagesAsync(100);

            pending.Should().HaveCount(1);
            pending[0].Status.Should().Be(OutboxMessageStatus.Pending);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Respect_BatchSize()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            for (int i = 0; i < 5; i++)
                await repo.AddAsync(BuildMessage());

            var pending = await repo.GetPendingMessagesAsync(3);

            pending.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Exclude_Exhausted_Retries()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage(maxRetries: 3);
            msg.RetryCount = 3; // exhausted
            await CreateRepository(ctx).AddAsync(msg);

            var pending = await CreateRepository(ctx).GetPendingMessagesAsync(100);

            pending.Should().BeEmpty();
        }

        // ---------------------------------------------------------------------------
        // MarkAsPublishedAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task MarkAsPublishedAsync_Should_Set_Published_Status()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var msg = BuildMessage();
            await repo.AddAsync(msg);

            await repo.MarkAsPublishedAsync(msg.Id);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
            saved!.Status.Should().Be(OutboxMessageStatus.Published);
            saved.PublishedAt.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // MarkAsFailedAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task MarkAsFailedAsync_Should_Increment_RetryCount()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var msg = BuildMessage(maxRetries: 5);
            await repo.AddAsync(msg);

            await repo.MarkAsFailedAsync(msg.Id, "Test error");
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
            saved!.RetryCount.Should().Be(1);
            saved.LastError.Should().Be("Test error");
            saved.NextRetryAt.Should().NotBeNull();
            saved.Status.Should().Be(OutboxMessageStatus.Pending); // not yet exhausted
        }

        [Fact]
        public async Task MarkAsFailedAsync_Should_Set_Failed_When_MaxRetries_Reached()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var msg = BuildMessage(maxRetries: 1);
            await repo.AddAsync(msg);

            await repo.MarkAsFailedAsync(msg.Id, "Final failure");
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
            saved!.Status.Should().Be(OutboxMessageStatus.Failed);
            saved.RetryCount.Should().Be(1);
        }

        // ---------------------------------------------------------------------------
        // GetByCorrelationIdAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task GetByCorrelationIdAsync_Should_Return_Message()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var correlationId = Guid.NewGuid();
            await repo.AddAsync(BuildMessage(correlationId));

            var result = await repo.GetByCorrelationIdAsync(correlationId);

            result.Should().NotBeNull();
            result!.CorrelationId.Should().Be(correlationId);
        }

        [Fact]
        public async Task GetByCorrelationIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepository(_fixture.CreateFreshDbContext())
                .GetByCorrelationIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        // ---------------------------------------------------------------------------
        // DeleteOldMessagesAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task DeleteOldMessagesAsync_Should_Delete_Old_Published_Messages()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            // Create a published message with old PublishedAt
            var msg = BuildMessage();
            await repo.AddAsync(msg);
            await repo.MarkAsPublishedAsync(msg.Id);

            // Manually set PublishedAt to 10 days ago
            await ctx.Database.ExecuteSqlRawAsync(
                "UPDATE Sales.OutboxMessages SET PublishedAt = DATEADD(DAY, -10, GETUTCDATE()) WHERE Id = {0}",
                msg.Id);
            ctx.ChangeTracker.Clear();

            await repo.DeleteOldMessagesAsync(daysToKeep: 7);
            ctx.ChangeTracker.Clear();

            var remaining = await ctx.OutboxMessages.CountAsync();
            remaining.Should().Be(0);
        }

        [Fact]
        public async Task DeleteOldMessagesAsync_Should_Not_Delete_Recent_Published()
        {
            await ClearOutboxAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var msg = BuildMessage();
            await repo.AddAsync(msg);
            await repo.MarkAsPublishedAsync(msg.Id);

            // PublishedAt is recent (just now) — should NOT be deleted
            await repo.DeleteOldMessagesAsync(daysToKeep: 7);
            ctx.ChangeTracker.Clear();

            var remaining = await ctx.OutboxMessages.CountAsync();
            remaining.Should().Be(1);
        }
    }
}
