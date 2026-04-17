using BackgroundService.Infrastructure.Persistence;
using BackgroundService.Infrastructure.Repositories.Inbox;
using BackgroundService.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackgroundService.IntegrationTests.Repositories.Inbox
{
    [Collection("DatabaseCollection")]
    public sealed class InboxRepositoryTests
    {
        private readonly DbFixture _fixture;

        public InboxRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private InboxRepository CreateRepo() =>
            new(_fixture.CreateFreshDbContext(), NullLogger<InboxRepository>.Instance);

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var rows = await ctx.InboxMessages.ToListAsync();
            ctx.InboxMessages.RemoveRange(rows);
            await ctx.SaveChangesAsync();
        }

        // --- IsAlreadyProcessedAsync ---

        [Fact]
        public async Task IsAlreadyProcessedAsync_ReturnsFalse_WhenNotProcessed()
        {
            await ClearAsync();

            var result = await CreateRepo().IsAlreadyProcessedAsync(
                "TestConsumer", Guid.NewGuid(), CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsAlreadyProcessedAsync_ReturnsTrue_AfterMarkAsProcessed()
        {
            await ClearAsync();
            var messageId = Guid.NewGuid();
            await CreateRepo().MarkAsProcessedAsync(
                "TestConsumer", messageId, Guid.NewGuid(), CancellationToken.None);

            var result = await CreateRepo().IsAlreadyProcessedAsync(
                "TestConsumer", messageId, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsAlreadyProcessedAsync_DistinguishesByConsumerName()
        {
            await ClearAsync();
            var messageId = Guid.NewGuid();
            await CreateRepo().MarkAsProcessedAsync(
                "ConsumerA", messageId, null, CancellationToken.None);

            var processedByB = await CreateRepo().IsAlreadyProcessedAsync(
                "ConsumerB", messageId, CancellationToken.None);

            processedByB.Should().BeFalse();
        }

        // --- MarkAsProcessedAsync ---

        [Fact]
        public async Task MarkAsProcessedAsync_Should_Persist_Entry()
        {
            await ClearAsync();
            var messageId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            await CreateRepo().MarkAsProcessedAsync(
                "PersistConsumer", messageId, correlationId, CancellationToken.None);

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var saved = await verifyCtx.InboxMessages
                .FirstOrDefaultAsync(m => m.ConsumerName == "PersistConsumer" && m.MessageId == messageId);
            saved.Should().NotBeNull();
            saved!.CorrelationId.Should().Be(correlationId);
            saved.ProcessedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        }

        [Fact]
        public async Task MarkAsProcessedAsync_Should_Persist_NullCorrelationId()
        {
            await ClearAsync();
            var messageId = Guid.NewGuid();

            await CreateRepo().MarkAsProcessedAsync(
                "NullCorrConsumer", messageId, null, CancellationToken.None);

            await using var nullVerifyCtx = _fixture.CreateFreshDbContext();
            var nullSaved = await nullVerifyCtx.InboxMessages
                .FirstAsync(m => m.MessageId == messageId);
            nullSaved.CorrelationId.Should().BeNull();
        }

        [Fact]
        public async Task MarkAsProcessedAsync_Should_Silently_Swallow_Duplicate_Inserts()
        {
            await ClearAsync();
            var messageId = Guid.NewGuid();

            await CreateRepo().MarkAsProcessedAsync(
                "DupConsumer", messageId, null, CancellationToken.None);

            // Second insert with same (ConsumerName, MessageId) should not throw
            var act = () => CreateRepo().MarkAsProcessedAsync(
                "DupConsumer", messageId, null, CancellationToken.None);

            await act.Should().NotThrowAsync();

            await using var countCtx = _fixture.CreateFreshDbContext();
            var count = await countCtx.InboxMessages
                .CountAsync(m => m.ConsumerName == "DupConsumer" && m.MessageId == messageId);
            count.Should().Be(1);
        }
    }
}
