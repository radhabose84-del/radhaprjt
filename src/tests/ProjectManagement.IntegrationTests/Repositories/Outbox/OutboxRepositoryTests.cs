using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectManagement.Domain.Entities.Outbox;
using ProjectManagement.Infrastructure.Repositories.Outbox;

namespace ProjectManagement.IntegrationTests.Repositories.Outbox
{
    [Collection("DatabaseCollection")]
    public sealed class OutboxRepositoryTests
    {
        private readonly DbFixture _fixture;

        public OutboxRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private static OutboxRepository CreateRepo(
            ProjectManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var loggerMock = new Mock<ILogger<OutboxRepository>>(MockBehavior.Loose);
            return new OutboxRepository(ctx, loggerMock.Object);
        }

        private static OutboxMessage BuildMessage(
            string eventType = "TestEvent",
            string eventData = "{\"key\":\"value\"}",
            Guid? correlationId = null) =>
            new OutboxMessage
            {
                CorrelationId = correlationId ?? Guid.NewGuid(),
                EventType = eventType,
                EventData = eventData,
                Status = OutboxMessageStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                MaxRetries = 5,
                ModuleName = "ProjectManagement"
            };

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        // --- AddAsync ---

        [Fact]
        public async Task AddAsync_Should_Persist_Message_With_Id()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var message = BuildMessage();
            await CreateRepo(ctx).AddAsync(message);

            message.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddAsync_Should_Set_Pending_Status()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var message = BuildMessage();
            await CreateRepo(ctx).AddAsync(message);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == message.Id);
            saved!.Status.Should().Be(OutboxMessageStatus.Pending);
        }

        [Fact]
        public async Task AddAsync_Should_Persist_Fields_Correctly()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var corrId = Guid.NewGuid();

            var message = BuildMessage("OrderCreated", "{\"orderId\":1}", corrId);
            await CreateRepo(ctx).AddAsync(message);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == message.Id);
            saved!.EventType.Should().Be("OrderCreated");
            saved.EventData.Should().Be("{\"orderId\":1}");
            saved.CorrelationId.Should().Be(corrId);
            saved.ModuleName.Should().Be("ProjectManagement");
        }

        // --- AddWithoutSaveAsync ---

        [Fact]
        public async Task AddWithoutSaveAsync_Should_Not_Persist_Until_SaveChanges()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var message = BuildMessage();
            await CreateRepo(ctx).AddWithoutSaveAsync(message);

            // Not yet persisted
            var count = await ctx.OutboxMessages.CountAsync();
            // The entity is tracked but Id may not be assigned until SaveChanges
            // We verify no rows in DB via a fresh context
            await using var ctx2 = _fixture.CreateFreshDbContext();
            var dbCount = await ctx2.OutboxMessages.CountAsync();
            dbCount.Should().Be(0);

            // Now save
            await ctx.SaveChangesAsync();
            await using var ctx3 = _fixture.CreateFreshDbContext();
            var finalCount = await ctx3.OutboxMessages.CountAsync();
            finalCount.Should().Be(1);
        }

        // --- GetPendingMessagesAsync ---

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Return_Pending_Messages()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepo(ctx).AddAsync(BuildMessage());
            await CreateRepo(ctx).AddAsync(BuildMessage());

            var result = await CreateRepo(ctx).GetPendingMessagesAsync();

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Exclude_Published_Messages()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            var msg = BuildMessage();
            await repo.AddAsync(msg);
            await repo.MarkAsPublishedAsync(msg.Id);
            await repo.AddAsync(BuildMessage()); // still pending

            var result = await repo.GetPendingMessagesAsync();

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Respect_BatchSize()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            for (int i = 0; i < 5; i++)
                await repo.AddAsync(BuildMessage());

            var result = await repo.GetPendingMessagesAsync(batchSize: 3);

            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Exclude_Messages_With_Future_NextRetryAt()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage();
            msg.NextRetryAt = DateTimeOffset.UtcNow.AddHours(1); // future
            await CreateRepo(ctx).AddAsync(msg);

            var result = await CreateRepo(ctx).GetPendingMessagesAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Exclude_Messages_At_MaxRetries()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var msg = BuildMessage();
            msg.RetryCount = 5; // equal to MaxRetries
            await CreateRepo(ctx).AddAsync(msg);

            var result = await CreateRepo(ctx).GetPendingMessagesAsync();

            result.Should().BeEmpty();
        }

        // --- MarkAsPublishedAsync ---

        [Fact]
        public async Task MarkAsPublishedAsync_Should_Set_Status_To_Published()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            var msg = BuildMessage();
            await repo.AddAsync(msg);

            await repo.MarkAsPublishedAsync(msg.Id);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
            saved!.Status.Should().Be(OutboxMessageStatus.Published);
            saved.PublishedAt.Should().NotBeNull();
            saved.LastError.Should().BeNull();
        }

        [Fact]
        public async Task MarkAsPublishedAsync_Should_Do_Nothing_When_NotFound()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            // Should not throw
            await CreateRepo(ctx).MarkAsPublishedAsync(99999);
        }

        // --- MarkAsFailedAsync ---

        [Fact]
        public async Task MarkAsFailedAsync_Should_Increment_RetryCount()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            var msg = BuildMessage();
            await repo.AddAsync(msg);

            await repo.MarkAsFailedAsync(msg.Id, "Connection timeout");
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
            saved!.RetryCount.Should().Be(1);
            saved.LastError.Should().Be("Connection timeout");
            saved.NextRetryAt.Should().NotBeNull();
        }

        [Fact]
        public async Task MarkAsFailedAsync_Should_Set_Status_Failed_When_MaxRetries_Reached()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            var msg = BuildMessage();
            msg.MaxRetries = 1;
            msg.RetryCount = 0;
            await repo.AddAsync(msg);

            await repo.MarkAsFailedAsync(msg.Id, "Final failure");
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
            saved!.Status.Should().Be(OutboxMessageStatus.Failed);
            saved.RetryCount.Should().Be(1);
        }

        [Fact]
        public async Task MarkAsFailedAsync_Should_Truncate_Long_ErrorMessage()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            var msg = BuildMessage();
            await repo.AddAsync(msg);

            var longError = new string('X', 3000);
            await repo.MarkAsFailedAsync(msg.Id, longError);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.OutboxMessages.FirstOrDefaultAsync(m => m.Id == msg.Id);
            saved!.LastError!.Length.Should().Be(2000);
        }

        // --- GetByCorrelationIdAsync ---

        [Fact]
        public async Task GetByCorrelationIdAsync_Should_Return_Matching_Message()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var corrId = Guid.NewGuid();

            await CreateRepo(ctx).AddAsync(BuildMessage(correlationId: corrId));

            var result = await CreateRepo(ctx).GetByCorrelationIdAsync(corrId);

            result.Should().NotBeNull();
            result!.CorrelationId.Should().Be(corrId);
        }

        [Fact]
        public async Task GetByCorrelationIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).GetByCorrelationIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }

        // --- DeleteOldMessagesAsync ---

        [Fact]
        public async Task DeleteOldMessagesAsync_Should_Remove_Old_Published_Messages()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            var msg = BuildMessage();
            await repo.AddAsync(msg);
            await repo.MarkAsPublishedAsync(msg.Id);

            // Manually set PublishedAt to 10 days ago
            var saved = await ctx.OutboxMessages.FirstAsync(m => m.Id == msg.Id);
            saved.PublishedAt = DateTimeOffset.UtcNow.AddDays(-10);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            await repo.DeleteOldMessagesAsync(daysToKeep: 7);

            var remaining = await ctx.OutboxMessages.CountAsync();
            remaining.Should().Be(0);
        }

        [Fact]
        public async Task DeleteOldMessagesAsync_Should_Keep_Recent_Published_Messages()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            var msg = BuildMessage();
            await repo.AddAsync(msg);
            await repo.MarkAsPublishedAsync(msg.Id); // published just now

            await repo.DeleteOldMessagesAsync(daysToKeep: 7);

            var remaining = await ctx.OutboxMessages.CountAsync();
            remaining.Should().Be(1);
        }

        [Fact]
        public async Task DeleteOldMessagesAsync_Should_Not_Delete_Pending_Messages()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepo(ctx);

            await repo.AddAsync(BuildMessage()); // pending, not published

            await repo.DeleteOldMessagesAsync(daysToKeep: 0); // even with 0 days

            var remaining = await ctx.OutboxMessages.CountAsync();
            remaining.Should().Be(1);
        }
    }
}
