using BudgetManagement.Domain.Entities.Outbox;
using BudgetManagement.Infrastructure.Repositories.Outbox;
using BudgetManagement.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BudgetManagement.IntegrationTests.Repositories.Outbox
{
    [Collection("DatabaseCollection")]
    public sealed class OutboxRepositoryTests
    {
        private readonly DbFixture _fixture;

        public OutboxRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private OutboxRepository CreateRepo() =>
            new(_fixture.CreateFreshDbContext(), NullLogger<OutboxRepository>.Instance);

        private static OutboxMessage BuildMessage(
            string eventType = "TestEvent",
            string data = "{}",
            Guid? correlationId = null,
            OutboxMessageStatus status = OutboxMessageStatus.Pending,
            int retryCount = 0,
            int maxRetries = 5,
            DateTimeOffset? nextRetryAt = null,
            DateTimeOffset? createdAt = null,
            DateTimeOffset? publishedAt = null) =>
            new()
            {
                CorrelationId = correlationId ?? Guid.NewGuid(),
                EventType = eventType,
                EventData = data,
                Status = status,
                RetryCount = retryCount,
                MaxRetries = maxRetries,
                NextRetryAt = nextRetryAt,
                CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
                PublishedAt = publishedAt,
                ModuleName = "BudgetManagement"
            };

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var rows = await ctx.OutboxMessages.ToListAsync();
            ctx.OutboxMessages.RemoveRange(rows);
            await ctx.SaveChangesAsync();
        }

        // --- AddAsync ---

        [Fact]
        public async Task AddAsync_Should_Persist_Message()
        {
            await ClearAsync();
            var correlationId = Guid.NewGuid();
            var msg = BuildMessage(correlationId: correlationId);

            await CreateRepo().AddAsync(msg);

            await using var ctx = _fixture.CreateFreshDbContext();
            (await ctx.OutboxMessages.AnyAsync(m => m.CorrelationId == correlationId)).Should().BeTrue();
        }

        // --- AddWithoutSaveAsync ---

        [Fact]
        public async Task AddWithoutSaveAsync_Should_Not_Save_Until_Caller_Does()
        {
            await ClearAsync();
            var correlationId = Guid.NewGuid();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new OutboxRepository(ctx, NullLogger<OutboxRepository>.Instance);

            await repo.AddWithoutSaveAsync(BuildMessage(correlationId: correlationId));

            // Not yet persisted in a separate context
            await using var verifyCtx = _fixture.CreateFreshDbContext();
            (await verifyCtx.OutboxMessages.AnyAsync(m => m.CorrelationId == correlationId)).Should().BeFalse();

            await ctx.SaveChangesAsync();

            await using var afterSave = _fixture.CreateFreshDbContext();
            (await afterSave.OutboxMessages.AnyAsync(m => m.CorrelationId == correlationId)).Should().BeTrue();
        }

        // --- GetPendingMessagesAsync ---

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Return_Pending_Messages()
        {
            await ClearAsync();
            await CreateRepo().AddAsync(BuildMessage(status: OutboxMessageStatus.Pending));
            await CreateRepo().AddAsync(BuildMessage(status: OutboxMessageStatus.Pending));
            await CreateRepo().AddAsync(BuildMessage(status: OutboxMessageStatus.Published));

            var result = await CreateRepo().GetPendingMessagesAsync();

            result.Should().HaveCount(2);
            result.Should().OnlyContain(m => m.Status == OutboxMessageStatus.Pending);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Exclude_Future_NextRetryAt()
        {
            await ClearAsync();
            await CreateRepo().AddAsync(BuildMessage(status: OutboxMessageStatus.Pending,
                nextRetryAt: DateTimeOffset.UtcNow.AddMinutes(10)));
            await CreateRepo().AddAsync(BuildMessage(status: OutboxMessageStatus.Pending, nextRetryAt: null));

            var result = await CreateRepo().GetPendingMessagesAsync();

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Respect_BatchSize()
        {
            await ClearAsync();
            for (int i = 0; i < 5; i++)
                await CreateRepo().AddAsync(BuildMessage());

            var result = await CreateRepo().GetPendingMessagesAsync(batchSize: 3);

            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Exclude_Exhausted_Retries()
        {
            await ClearAsync();
            await CreateRepo().AddAsync(BuildMessage(retryCount: 5, maxRetries: 5));
            await CreateRepo().AddAsync(BuildMessage(retryCount: 0, maxRetries: 5));

            var result = await CreateRepo().GetPendingMessagesAsync();

            result.Should().HaveCount(1);
        }

        // --- MarkAsPublishedAsync ---

        [Fact]
        public async Task MarkAsPublishedAsync_Should_Set_Status_Published()
        {
            await ClearAsync();
            var msg = BuildMessage();
            await CreateRepo().AddAsync(msg);

            await CreateRepo().MarkAsPublishedAsync(msg.Id);

            await using var ctx = _fixture.CreateFreshDbContext();
            var updated = await ctx.OutboxMessages.FirstAsync(m => m.Id == msg.Id);
            updated.Status.Should().Be(OutboxMessageStatus.Published);
            updated.PublishedAt.Should().NotBeNull();
            updated.LastError.Should().BeNull();
        }

        [Fact]
        public async Task MarkAsPublishedAsync_Should_NoOp_When_MessageId_NotFound()
        {
            await ClearAsync();

            var act = () => CreateRepo().MarkAsPublishedAsync(99999);

            await act.Should().NotThrowAsync();
        }

        // --- MarkAsFailedAsync ---

        [Fact]
        public async Task MarkAsFailedAsync_Should_Increment_Retry_And_Update_Error()
        {
            await ClearAsync();
            var msg = BuildMessage();
            await CreateRepo().AddAsync(msg);

            await CreateRepo().MarkAsFailedAsync(msg.Id, "boom");

            await using var ctx = _fixture.CreateFreshDbContext();
            var updated = await ctx.OutboxMessages.FirstAsync(m => m.Id == msg.Id);
            updated.RetryCount.Should().Be(1);
            updated.LastError.Should().Be("boom");
            updated.NextRetryAt.Should().NotBeNull();
        }

        [Fact]
        public async Task MarkAsFailedAsync_Should_Mark_Failed_Status_When_Retries_Exhausted()
        {
            await ClearAsync();
            var msg = BuildMessage(retryCount: 4, maxRetries: 5);
            await CreateRepo().AddAsync(msg);

            await CreateRepo().MarkAsFailedAsync(msg.Id, "final");

            await using var ctx = _fixture.CreateFreshDbContext();
            var updated = await ctx.OutboxMessages.FirstAsync(m => m.Id == msg.Id);
            updated.Status.Should().Be(OutboxMessageStatus.Failed);
            updated.RetryCount.Should().Be(5);
        }

        [Fact]
        public async Task MarkAsFailedAsync_Should_Truncate_LongError()
        {
            await ClearAsync();
            var msg = BuildMessage();
            await CreateRepo().AddAsync(msg);
            var longError = new string('x', 5000);

            await CreateRepo().MarkAsFailedAsync(msg.Id, longError);

            await using var ctx = _fixture.CreateFreshDbContext();
            var updated = await ctx.OutboxMessages.FirstAsync(m => m.Id == msg.Id);
            updated.LastError!.Length.Should().Be(2000);
        }

        // --- DeleteOldMessagesAsync ---

        [Fact]
        public async Task DeleteOldMessagesAsync_Should_Remove_Published_Older_Than_Cutoff()
        {
            await ClearAsync();
            await CreateRepo().AddAsync(BuildMessage(status: OutboxMessageStatus.Published,
                publishedAt: DateTimeOffset.UtcNow.AddDays(-30)));
            await CreateRepo().AddAsync(BuildMessage(status: OutboxMessageStatus.Published,
                publishedAt: DateTimeOffset.UtcNow.AddDays(-1)));

            await CreateRepo().DeleteOldMessagesAsync(daysToKeep: 7);

            await using var ctx = _fixture.CreateFreshDbContext();
            (await ctx.OutboxMessages.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task DeleteOldMessagesAsync_Should_Preserve_Pending_Messages()
        {
            await ClearAsync();
            await CreateRepo().AddAsync(BuildMessage(status: OutboxMessageStatus.Pending));

            await CreateRepo().DeleteOldMessagesAsync(daysToKeep: 7);

            await using var ctx = _fixture.CreateFreshDbContext();
            (await ctx.OutboxMessages.CountAsync()).Should().Be(1);
        }

        // --- GetByCorrelationIdAsync ---

        [Fact]
        public async Task GetByCorrelationIdAsync_Should_Return_Matching_Message()
        {
            await ClearAsync();
            var correlationId = Guid.NewGuid();
            await CreateRepo().AddAsync(BuildMessage(correlationId: correlationId));

            var result = await CreateRepo().GetByCorrelationIdAsync(correlationId);

            result.Should().NotBeNull();
            result!.CorrelationId.Should().Be(correlationId);
        }

        [Fact]
        public async Task GetByCorrelationIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearAsync();

            var result = await CreateRepo().GetByCorrelationIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }
    }
}
