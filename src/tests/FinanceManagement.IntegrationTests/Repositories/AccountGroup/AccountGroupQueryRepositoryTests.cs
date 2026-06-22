using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.AccountGroup;

namespace FinanceManagement.IntegrationTests.Repositories.AccountGroup
{
    // Query-side: the real Dapper CTEs against a live SQL test DB —
    // the nested tree build, descendant detection, and the leaf-groups query.
    [Collection("DatabaseCollection")]
    public sealed class AccountGroupQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public AccountGroupQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private AccountGroupQueryRepository QueryRepo()
        {
            var company = new Mock<ICompanyLookup>(MockBehavior.Loose);
            company.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Test Co" } });
            return new AccountGroupQueryRepository(new SqlConnection(_fixture.ConnectionString), company.Object);
        }

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

        [Fact]
        public async Task GetTree_BuildsNestedShape_WithLeafOnlyAtBottom()
        {
            await _fixture.ClearAllTablesAsync();

            var a = await CreateAsync("A", "Asset", null);
            var ca = await CreateAsync("A-CA", "Current Assets", a);
            var inv = await CreateAsync("A-CA-INV", "Inventories", ca);

            var roots = await QueryRepo().GetTreeAsync(1);

            roots.Should().ContainSingle();
            var root = roots[0];
            root.Id.Should().Be(a);
            root.IsLeaf.Should().BeFalse();
            root.Children.Should().ContainSingle(x => x.Id == ca);
            var inventories = root.Children[0].Children.Single(x => x.Id == inv);
            inventories.Level.Should().Be(3);
            inventories.IsLeaf.Should().BeTrue("only the bottom node is a leaf");
        }

        [Fact]
        public async Task IsDescendant_DetectsSubtreeMembership()
        {
            await _fixture.ClearAllTablesAsync();

            var a = await CreateAsync("A", "Asset", null);
            var ca = await CreateAsync("A-CA", "Current Assets", a);
            var inv = await CreateAsync("A-CA-INV", "Inventories", ca);

            (await QueryRepo().IsDescendantAsync(a, inv)).Should().BeTrue("INV is under A");
            (await QueryRepo().IsDescendantAsync(inv, a)).Should().BeFalse("A is not under INV");
        }

        [Fact]
        public async Task GetLeafGroups_ReturnsOnlyLeaves()
        {
            await _fixture.ClearAllTablesAsync();

            var a = await CreateAsync("A", "Asset", null);
            var ca = await CreateAsync("A-CA", "Current Assets", a);
            var inv = await CreateAsync("A-CA-INV", "Inventories", ca);   // leaf

            var leaves = await QueryRepo().GetLeafGroupsAsync(1, null);
            var ids = leaves.Select(x => x.Id).ToList();

            ids.Should().Contain(inv);
            ids.Should().NotContain(a);
            ids.Should().NotContain(ca);
        }
    }
}
