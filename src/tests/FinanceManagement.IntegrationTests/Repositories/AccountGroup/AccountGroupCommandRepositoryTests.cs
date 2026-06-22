using Dapper;
using Microsoft.Data.SqlClient;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.AccountGroup;

namespace FinanceManagement.IntegrationTests.Repositories.AccountGroup
{
    // Command-side: EF derivation logic against a live SQL test DB —
    // Level derivation, IsLeaf maintenance on create, and the Move leaf-flag cascade.
    [Collection("DatabaseCollection")]
    public sealed class AccountGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public AccountGroupCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private async Task<int> CreateAsync(string code, string name, int? parentId)
        {
            using var ctx = _fixture.CreateFreshDbContext();
            var repo = new AccountGroupCommandRepository(ctx);
            return await repo.CreateAsync(new FinanceManagement.Domain.Entities.AccountGroup
            {
                CompanyId = 1,
                GroupCode = code,
                GroupName = name,
                ParentAccountGroupId = parentId,
                SortOrder = 1
            });
        }

        private sealed record Row(int Level, bool IsLeaf, int? ParentAccountGroupId);

        private async Task<Row> ReadAsync(int id)
        {
            await using var c = new SqlConnection(_fixture.ConnectionString);
            return await c.QuerySingleAsync<Row>(
                "SELECT [Level], IsLeaf, ParentAccountGroupId FROM Finance.AccountGroup WHERE Id = @id", new { id });
        }

        [Fact]
        public async Task Create_DerivesLevel_AndFlipsParentLeaf()
        {
            await _fixture.ClearAllTablesAsync();

            var rootId = await CreateAsync("A", "Asset", null);
            var childId = await CreateAsync("A-CA", "Current Assets", rootId);

            var root = await ReadAsync(rootId);
            var child = await ReadAsync(childId);

            root.Level.Should().Be(1);
            root.IsLeaf.Should().BeFalse("root gained a child");
            child.Level.Should().Be(2, "Level = parent.Level + 1");
            child.IsLeaf.Should().BeTrue("new node has no children");
        }

        [Fact]
        public async Task Move_FlipsLeafFlagsOnOldAndNewParent()
        {
            await _fixture.ClearAllTablesAsync();

            var a = await CreateAsync("A", "Asset", null);
            var ca = await CreateAsync("A-CA", "Current Assets", a);     // will lose its child
            var nca = await CreateAsync("A-NCA", "Non-Current Assets", a); // will gain a child
            var inv = await CreateAsync("A-CA-INV", "Inventories", ca);

            using (var ctx = _fixture.CreateFreshDbContext())
            {
                var repo = new AccountGroupCommandRepository(ctx);
                await repo.MoveAsync(inv, nca);
            }

            (await ReadAsync(inv)).ParentAccountGroupId.Should().Be(nca, "the node is re-parented");
            (await ReadAsync(ca)).IsLeaf.Should().BeTrue("old parent lost its only child");
            (await ReadAsync(nca)).IsLeaf.Should().BeFalse("new parent gained a child");
        }
    }
}
