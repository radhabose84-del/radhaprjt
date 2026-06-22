using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.Infrastructure.Repositories.CoaFreeze;
using FinanceManagement.IntegrationTests.Common;

namespace FinanceManagement.IntegrationTests.Repositories.CoaFreeze
{
    // US-GL02-FR-008a — freeze-state command/query repos against the real test DB.
    // The DB triggers themselves are NOT created by EnsureCreated (they ship in a migration),
    // so AreTriggersActiveAsync is expected FALSE here — documented below.
    [Collection("DatabaseCollection")]
    public sealed class CoaFreezeRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CoaFreezeRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private static CoaFreezeCommandRepository CmdRepo(ApplicationDbContext ctx) => new(ctx);
        private CoaFreezeQueryRepository QueryRepo() => new(new SqlConnection(_fixture.ConnectionString));

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Finance.CoaFreezeState");
        }

        [Fact]
        public async Task FreezeAsync_Persists_Frozen_Row()
        {
            await ClearAsync();
            var now = DateTimeOffset.UtcNow;

            await using (var ctx = _fixture.CreateFreshDbContext())
                await CmdRepo(ctx).FreezeAsync(companyId: 1, frozenByUserId: 396, frozenOn: now, CancellationToken.None);

            await using var read = _fixture.CreateFreshDbContext();
            var row = await read.CoaFreezeState.FirstOrDefaultAsync(x => x.CompanyId == 1);

            row.Should().NotBeNull();
            row!.IsFrozen.Should().BeTrue();
            row.FrozenByUserId.Should().Be(396);
            row.FrozenOn.Should().NotBeNull();
            row.UnfreezeWindowExpiry.Should().BeNull();
        }

        [Fact]
        public async Task OpenUnfreezeWindowAsync_SetsNotFrozen_WithExpiry()
        {
            await ClearAsync();
            var now = DateTimeOffset.UtcNow;
            var expiry = now.AddMinutes(60);

            await using (var ctx = _fixture.CreateFreshDbContext())
                await CmdRepo(ctx).FreezeAsync(1, 396, now, CancellationToken.None);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CmdRepo(ctx).OpenUnfreezeWindowAsync(1, expiry, CancellationToken.None);

            await using var read = _fixture.CreateFreshDbContext();
            var row = await read.CoaFreezeState.FirstAsync(x => x.CompanyId == 1);

            row.IsFrozen.Should().BeFalse();
            row.UnfreezeWindowExpiry.Should().NotBeNull();
            row.FrozenOn.Should().NotBeNull("the original freeze time is retained as history");
        }

        [Fact]
        public async Task Freeze_IsUpsert_OneRowPerCompany()
        {
            await ClearAsync();
            var now = DateTimeOffset.UtcNow;

            await using (var ctx = _fixture.CreateFreshDbContext())
                await CmdRepo(ctx).FreezeAsync(1, 396, now, CancellationToken.None);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CmdRepo(ctx).FreezeAsync(1, 400, now.AddMinutes(5), CancellationToken.None);

            await using var read = _fixture.CreateFreshDbContext();
            var count = await read.CoaFreezeState.CountAsync(x => x.CompanyId == 1);
            count.Should().Be(1);
        }

        [Fact]
        public async Task GetStateAsync_ReturnsFrozenRow()
        {
            await ClearAsync();
            var now = DateTimeOffset.UtcNow;
            await using (var ctx = _fixture.CreateFreshDbContext())
                await CmdRepo(ctx).FreezeAsync(1, 396, now, CancellationToken.None);

            var state = await QueryRepo().GetStateAsync(1, CancellationToken.None);

            state.Should().NotBeNull();
            state!.IsFrozen.Should().BeTrue();
            state.FrozenByUserId.Should().Be(396);
        }

        [Fact]
        public async Task AreTriggersActiveAsync_FalseInEnsureCreatedDb()
        {
            // EnsureCreated builds tables from the model but NOT the migration triggers, so this
            // documents that the engine reports "DB Trigger: INACTIVE" until the migration is applied.
            var active = await QueryRepo().AreTriggersActiveAsync(CancellationToken.None);
            active.Should().BeFalse();
        }
    }
}
