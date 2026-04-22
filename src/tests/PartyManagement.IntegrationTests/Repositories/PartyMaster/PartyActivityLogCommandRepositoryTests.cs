using Microsoft.EntityFrameworkCore;
using PartyManagement.Domain.Entities;
using PartyManagement.Infrastructure.Repositories.PartyMaster;

namespace PartyManagement.IntegrationTests.Repositories.PartyMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PartyActivityLogCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyActivityLogCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyActivityLogCommandRepository CreateRepository(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static PartyActivityLog BuildLog(
            int partyId = 1,
            string tableName = "PartyMaster",
            string columnName = "PartyName",
            string oldValue = "Old Name",
            string newValue = "New Name",
            string actionType = "Update") =>
            new PartyActivityLog
            {
                PartyId = partyId,
                TableName = tableName,
                ColumnName = columnName,
                OldValue = oldValue,
                NewValue = newValue,
                ActionType = actionType,
                ChangedBy = 1,
                ChangedByName = "test-user",
                ChangedIp = "127.0.0.1",
                ChangedOn = DateTimeOffset.UtcNow
            };

        // --- INSERT ---

        [Fact]
        public async Task InsertAsync_Should_Return_GreaterThanZero()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).InsertAsync(BuildLog());

            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task InsertAsync_Should_Persist_Log()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            await CreateRepository(ctx).InsertAsync(BuildLog(partyId: 5, tableName: "PartyContacts", actionType: "Insert"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PartyActivityLog.FirstOrDefaultAsync(l => l.PartyId == 5);

            saved.Should().NotBeNull();
            saved!.TableName.Should().Be("PartyContacts");
            saved.ActionType.Should().Be("Insert");
            saved.ChangedBy.Should().Be(1);
        }

        [Fact]
        public async Task InsertAsync_Should_Persist_AllFields_Correctly()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var log = BuildLog(
                partyId: 10,
                tableName: "PartyDocuments",
                columnName: "FileName",
                oldValue: "old.pdf",
                newValue: "new.pdf",
                actionType: "Delete");

            await CreateRepository(ctx).InsertAsync(log);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PartyActivityLog.FirstOrDefaultAsync(l => l.PartyId == 10);

            saved.Should().NotBeNull();
            saved!.ColumnName.Should().Be("FileName");
            saved.OldValue.Should().Be("old.pdf");
            saved.NewValue.Should().Be("new.pdf");
            saved.ChangedByName.Should().Be("test-user");
            saved.ChangedIp.Should().Be("127.0.0.1");
        }

        // --- GET ACTIVITY LOGS BY PARTY ID ---

        [Fact]
        public async Task GetActivityLogsByPartyIdAsync_Should_Return_Logs_For_PartyId()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            await repo.InsertAsync(BuildLog(partyId: 20, actionType: "Insert"));
            await repo.InsertAsync(BuildLog(partyId: 20, actionType: "Update"));
            await repo.InsertAsync(BuildLog(partyId: 30, actionType: "Insert")); // different party

            var logs = await repo.GetActivityLogsByPartyIdAsync(20, CancellationToken.None);

            logs.Should().HaveCount(2);
            logs.Should().OnlyContain(l => l.PartyId == 20);
        }

        [Fact]
        public async Task GetActivityLogsByPartyIdAsync_NoLogs_ReturnsEmptyList()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var logs = await CreateRepository(ctx).GetActivityLogsByPartyIdAsync(999, CancellationToken.None);

            logs.Should().BeEmpty();
        }

        [Fact]
        public async Task GetActivityLogsByPartyIdAsync_OrderedByChangedOnDescending()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var log1 = BuildLog(partyId: 40, actionType: "Insert");
            log1.ChangedOn = DateTimeOffset.UtcNow.AddHours(-2);
            await repo.InsertAsync(log1);

            var log2 = BuildLog(partyId: 40, actionType: "Update");
            log2.ChangedOn = DateTimeOffset.UtcNow;
            await repo.InsertAsync(log2);

            var logs = await repo.GetActivityLogsByPartyIdAsync(40, CancellationToken.None);

            logs.Should().HaveCount(2);
            logs[0].ChangedOn.Should().BeOnOrAfter(logs[1].ChangedOn);
        }
    }
}
