using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Domain.Entities.Outbox;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.Outbox;

namespace PurchaseManagement.IntegrationTests.Repositories.Outbox
{
    [Collection("DatabaseCollection")]
    public sealed class OutboxRepositoryTests
    {
        private readonly DbFixture _fixture;

        public OutboxRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private OutboxRepository CreateRepo(ApplicationDbContext ctx) =>
            new(ctx, new Mock<ILogger<OutboxRepository>>(MockBehavior.Loose).Object);

        private static OutboxMessage BuildMessage(
            string eventType = "TestEvent",
            string eventData = "{\"key\":\"value\"}",
            OutboxMessageStatus status = OutboxMessageStatus.Pending) =>
            new()
            {
                CorrelationId = Guid.NewGuid(),
                EventType = eventType,
                EventData = eventData,
                Status = status,
                CreatedAt = DateTimeOffset.UtcNow,
                RetryCount = 0,
                MaxRetries = 5,
                ModuleName = "PurchaseManagement",
                CreatedBy = 1
            };

        // --- ADD ---

        [Fact]
        public async Task AddAsync_Should_Persist_Message()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage();
            await CreateRepo(ctx).AddAsync(msg);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync();
            saved.Should().NotBeNull();
            saved!.EventType.Should().Be("TestEvent");
            saved.Status.Should().Be(OutboxMessageStatus.Pending);
        }

        [Fact]
        public async Task AddAsync_Should_Assign_Id()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage();
            await CreateRepo(ctx).AddAsync(msg);

            msg.Id.Should().BeGreaterThan(0);
        }

        // --- ADD WITHOUT SAVE ---

        [Fact]
        public async Task AddWithoutSaveAsync_Should_Not_Persist_Until_SaveChanges()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage();
            await CreateRepo(ctx).AddWithoutSaveAsync(msg);

            // Not yet saved - check via separate context
            await using var ctx2 = _fixture.CreateFreshDbContext();
            var count = await ctx2.OutboxMessages.CountAsync();
            count.Should().Be(0);

            // Now save
            await ctx.SaveChangesAsync();
            await using var ctx3 = _fixture.CreateFreshDbContext();
            var countAfter = await ctx3.OutboxMessages.CountAsync();
            countAfter.Should().Be(1);
        }

        // --- GET PENDING ---

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Return_Only_Pending()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var pending = BuildMessage(status: OutboxMessageStatus.Pending);
            var published = BuildMessage(eventType: "PublishedEvent", status: OutboxMessageStatus.Published);
            published.PublishedAt = DateTimeOffset.UtcNow;

            await ctx.OutboxMessages.AddRangeAsync(pending, published);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var results = await CreateRepo(ctx).GetPendingMessagesAsync();

            results.Should().HaveCount(1);
            results[0].EventType.Should().Be("TestEvent");
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Respect_BatchSize()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            for (int i = 0; i < 5; i++)
            {
                var msg = BuildMessage(eventType: $"Event{i}");
                await ctx.OutboxMessages.AddAsync(msg);
            }
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var results = await CreateRepo(ctx).GetPendingMessagesAsync(batchSize: 3);

            results.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Skip_MaxRetries_Exceeded()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage();
            msg.RetryCount = 5;
            msg.MaxRetries = 5;
            await ctx.OutboxMessages.AddAsync(msg);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var results = await CreateRepo(ctx).GetPendingMessagesAsync();

            results.Should().BeEmpty();
        }

        // --- MARK AS PUBLISHED ---

        [Fact]
        public async Task MarkAsPublishedAsync_Should_Update_Status()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage();
            await ctx.OutboxMessages.AddAsync(msg);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).MarkAsPublishedAsync(msg.Id);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstAsync(m => m.Id == msg.Id);
            saved.Status.Should().Be(OutboxMessageStatus.Published);
            saved.PublishedAt.Should().NotBeNull();
            saved.LastError.Should().BeNull();
        }

        // --- MARK AS FAILED ---

        [Fact]
        public async Task MarkAsFailedAsync_Should_Increment_RetryCount()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage();
            await ctx.OutboxMessages.AddAsync(msg);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).MarkAsFailedAsync(msg.Id, "Test error");
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstAsync(m => m.Id == msg.Id);
            saved.RetryCount.Should().Be(1);
            saved.LastError.Should().Be("Test error");
            saved.NextRetryAt.Should().NotBeNull();
        }

        [Fact]
        public async Task MarkAsFailedAsync_Should_Set_Failed_Status_On_MaxRetries()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage();
            msg.RetryCount = 4; // One more will hit MaxRetries=5
            msg.MaxRetries = 5;
            await ctx.OutboxMessages.AddAsync(msg);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).MarkAsFailedAsync(msg.Id, "Final failure");
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstAsync(m => m.Id == msg.Id);
            saved.Status.Should().Be(OutboxMessageStatus.Failed);
            saved.RetryCount.Should().Be(5);
        }

        // --- GET BY CORRELATION ID ---

        [Fact]
        public async Task GetByCorrelationIdAsync_Should_Return_Correct_Message()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var correlationId = Guid.NewGuid();
            var msg = BuildMessage();
            msg.CorrelationId = correlationId;
            await ctx.OutboxMessages.AddAsync(msg);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).GetByCorrelationIdAsync(correlationId);

            result.Should().NotBeNull();
            result!.CorrelationId.Should().Be(correlationId);
        }

        [Fact]
        public async Task GetByCorrelationIdAsync_Should_Return_Null_When_NotFound()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetByCorrelationIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        // --- DELETE OLD MESSAGES ---

        [Fact]
        public async Task DeleteOldMessagesAsync_Should_Remove_Old_Published_Messages()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var oldMsg = BuildMessage(status: OutboxMessageStatus.Published);
            oldMsg.PublishedAt = DateTimeOffset.UtcNow.AddDays(-10);
            var recentMsg = BuildMessage(eventType: "RecentEvent", status: OutboxMessageStatus.Published);
            recentMsg.PublishedAt = DateTimeOffset.UtcNow;

            await ctx.OutboxMessages.AddRangeAsync(oldMsg, recentMsg);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).DeleteOldMessagesAsync(daysToKeep: 7);

            var remaining = await ctx.OutboxMessages.ToListAsync();
            remaining.Should().HaveCount(1);
            remaining[0].EventType.Should().Be("RecentEvent");
        }
    }
}
