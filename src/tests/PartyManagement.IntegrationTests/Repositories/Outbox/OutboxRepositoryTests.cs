using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PartyManagement.Domain.Entities.Outbox;
using PartyManagement.Infrastructure.Repositories.Outbox;

namespace PartyManagement.IntegrationTests.Repositories.Outbox
{
    [Collection("DatabaseCollection")]
    public sealed class OutboxRepositoryTests
    {
        private readonly DbFixture _fixture;

        public OutboxRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private OutboxRepository CreateRepository(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var logger = new Mock<ILogger<OutboxRepository>>(MockBehavior.Loose);
            return new OutboxRepository(ctx, logger.Object);
        }

        private static OutboxMessage BuildMessage(
            string eventType = "PartyCreated",
            string eventData = "{\"partyId\":1}") =>
            new OutboxMessage
            {
                CorrelationId = Guid.NewGuid(),
                EventType = eventType,
                EventData = eventData,
                Status = OutboxMessageStatus.Pending,
                CreatedAt = DateTimeOffset.UtcNow,
                MaxRetries = 5,
                ModuleName = "PartyManagement",
                CreatedBy = 1
            };

        // --- ADD ---

        [Fact]
        public async Task AddAsync_Should_Persist_Message()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var message = BuildMessage();
            await repo.AddAsync(message);

            message.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddAsync_Should_Persist_Fields_Correctly()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var correlationId = Guid.NewGuid();
            var message = BuildMessage();
            message.CorrelationId = correlationId;
            await repo.AddAsync(message);

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var saved = await ctx2.OutboxMessages.FirstOrDefaultAsync(m => m.CorrelationId == correlationId);

            saved.Should().NotBeNull();
            saved!.EventType.Should().Be("PartyCreated");
            saved.Status.Should().Be(OutboxMessageStatus.Pending);
            saved.ModuleName.Should().Be("PartyManagement");
        }

        // --- GET PENDING ---

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Return_Pending_Messages()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            await repo.AddAsync(BuildMessage());
            await repo.AddAsync(BuildMessage("PartyUpdated", "{\"partyId\":2}"));

            var pending = await repo.GetPendingMessagesAsync();

            pending.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Exclude_Published_Messages()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var message = BuildMessage();
            await repo.AddAsync(message);
            await repo.MarkAsPublishedAsync(message.Id);

            var pending = await repo.GetPendingMessagesAsync();

            pending.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPendingMessagesAsync_Should_Respect_BatchSize()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            for (int i = 0; i < 5; i++)
                await repo.AddAsync(BuildMessage($"Event{i}"));

            var pending = await repo.GetPendingMessagesAsync(batchSize: 3);

            pending.Should().HaveCount(3);
        }

        // --- MARK AS PUBLISHED ---

        [Fact]
        public async Task MarkAsPublishedAsync_Should_Update_Status()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var message = BuildMessage();
            await repo.AddAsync(message);
            await repo.MarkAsPublishedAsync(message.Id);

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var updated = await ctx2.OutboxMessages.FirstOrDefaultAsync(m => m.Id == message.Id);

            updated!.Status.Should().Be(OutboxMessageStatus.Published);
            updated.PublishedAt.Should().NotBeNull();
        }

        // --- MARK AS FAILED ---

        [Fact]
        public async Task MarkAsFailedAsync_Should_Increment_RetryCount()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var message = BuildMessage();
            await repo.AddAsync(message);
            await repo.MarkAsFailedAsync(message.Id, "Connection timeout");

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var updated = await ctx2.OutboxMessages.FirstOrDefaultAsync(m => m.Id == message.Id);

            updated!.RetryCount.Should().Be(1);
            updated.LastError.Should().Be("Connection timeout");
        }

        [Fact]
        public async Task MarkAsFailedAsync_ExceedMaxRetries_SetsStatusToFailed()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var message = BuildMessage();
            message.MaxRetries = 1;
            await repo.AddAsync(message);
            await repo.MarkAsFailedAsync(message.Id, "Error");

            await using var ctx2 = _fixture.CreateFreshDbContext();
            var updated = await ctx2.OutboxMessages.FirstOrDefaultAsync(m => m.Id == message.Id);

            updated!.Status.Should().Be(OutboxMessageStatus.Failed);
        }

        // --- GET BY CORRELATION ID ---

        [Fact]
        public async Task GetByCorrelationIdAsync_Should_Return_Correct_Message()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var correlationId = Guid.NewGuid();
            var message = BuildMessage();
            message.CorrelationId = correlationId;
            await repo.AddAsync(message);

            var result = await repo.GetByCorrelationIdAsync(correlationId);

            result.Should().NotBeNull();
            result!.CorrelationId.Should().Be(correlationId);
        }

        [Fact]
        public async Task GetByCorrelationIdAsync_NonExistent_ReturnsNull()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var result = await repo.GetByCorrelationIdAsync(Guid.NewGuid());

            result.Should().BeNull();
        }
    }
}
