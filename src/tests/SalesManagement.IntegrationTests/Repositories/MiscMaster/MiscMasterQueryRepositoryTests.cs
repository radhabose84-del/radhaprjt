using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.MiscMaster;
using SalesManagement.Infrastructure.Repositories.MiscTypeMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.MiscMaster
{
    /// <summary>
    /// Integration tests for MiscMasterQueryRepository.
    /// Verifies Dapper SQL queries against a real SQL Server database.
    /// MiscTypeMaster rows are seeded as FK prerequisites.
    /// All queries JOIN to MiscTypeMaster — both tables must be populated.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private SqlConnection OpenConnection() => new SqlConnection(_fixture.ConnectionString);

        private MiscMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscMasterQueryRepository(conn);
        }

        private MiscMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new MiscMasterCommandRepository(ctx);

        private MiscTypeMasterCommandRepository CreateTypeCommandRepo(ApplicationDbContext ctx)
            => new MiscTypeMasterCommandRepository(ctx);

        private async Task ClearMiscMasterTableAsync()
        {
            await using var cnn = OpenConnection();
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.MiscMaster");
        }

        /// <summary>Ensures a MiscTypeMaster row exists for the given code; returns its Id.</summary>
        private async Task<int> EnsureMiscTypeExistsAsync(string code = "MISC001")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscTypeMaster
                .FirstOrDefaultAsync(x => x.MiscTypeCode == code && x.IsDeleted == IsDelete.NotDeleted);

            if (existing != null)
                return existing.Id;

            return await CreateTypeCommandRepo(ctx).CreateAsync(new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = $"Fixture Misc Type {code}",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedMiscMasterAsync(
            int miscTypeId,
            string code = "CODE001",
            string description = "Test Item",
            int sortOrder = 1,
            bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Records()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QALL01");
            await SeedMiscMasterAsync(miscTypeId, "CODE001", "Item One");
            await SeedMiscMasterAsync(miscTypeId, "CODE002", "Item Two");

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            totalCount.Should().Be(2);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_MiscTypeCode()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QTYPECODE");
            await SeedMiscMasterAsync(miscTypeId, "CODE001", "Item");

            var (data, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().NotBeEmpty();
            data[0].MiscTypeCode.Should().Be("QTYPECODE");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QSEARCH01");
            await SeedMiscMasterAsync(miscTypeId, "ALPHA01", "Alpha Item");
            await SeedMiscMasterAsync(miscTypeId, "BETA01", "Beta Item");

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            totalCount.Should().Be(1);
            data[0].Code.Should().Be("ALPHA01");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_MiscTypeId()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId1 = await EnsureMiscTypeExistsAsync("QFILTER01");
            var miscTypeId2 = await EnsureMiscTypeExistsAsync("QFILTER02");
            await SeedMiscMasterAsync(miscTypeId1, "T1CODE01", "Type1 Item");
            await SeedMiscMasterAsync(miscTypeId2, "T2CODE01", "Type2 Item");

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null, miscTypeId1);

            totalCount.Should().Be(1);
            data[0].MiscTypeId.Should().Be(miscTypeId1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QDEL01");
            var id = await SeedMiscMasterAsync(miscTypeId, "DELCODE01", "To Be Deleted");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (data, totalCount) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.Code == "DELCODE01");
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QPAGE01");
            for (int i = 1; i <= 5; i++)
                await SeedMiscMasterAsync(miscTypeId, $"PC{i:D3}", $"Item {i}", sortOrder: i);

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 3, null);
            var (page2, _) = await CreateQueryRepo().GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            var page1Codes = page1.Select(x => x.Code).ToList();
            var page2Codes = page2.Select(x => x.Code).ToList();
            page1Codes.Should().NotIntersectWith(page2Codes);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QBYID01");
            var id = await SeedMiscMasterAsync(miscTypeId, "BYID01", "ById Item", sortOrder: 3);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.Code.Should().Be("BYID01");
            dto.Description.Should().Be("ById Item");
            dto.SortOrder.Should().Be(3);
            dto.MiscTypeId.Should().Be(miscTypeId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_MiscTypeCode()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QBYIDLK");
            var id = await SeedMiscMasterAsync(miscTypeId, "BYLK01", "Lookup Item");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto!.MiscTypeCode.Should().Be("QBYIDLK");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearMiscMasterTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QBYDEL01");
            var id = await SeedMiscMasterAsync(miscTypeId, "SDEL01", "Soft Deleted");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // ── AlreadyExistsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenCodeExistsForSameMiscType()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QEXIST01");
            await SeedMiscMasterAsync(miscTypeId, "EXISTS01", "Item");

            var result = await CreateQueryRepo().AlreadyExistsAsync("EXISTS01", miscTypeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenCodeExistsForDifferentMiscType()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId1 = await EnsureMiscTypeExistsAsync("QEXIST02");
            var miscTypeId2 = await EnsureMiscTypeExistsAsync("QEXIST03");
            await SeedMiscMasterAsync(miscTypeId1, "SAMECODE", "Item for Type1");

            // Same code, different miscTypeId — should NOT exist for type2
            var result = await CreateQueryRepo().AlreadyExistsAsync("SAMECODE", miscTypeId2);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenExcludedId_Matches()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QEXCL01");
            var id = await SeedMiscMasterAsync(miscTypeId, "EXCL01", "Item");

            // Excluding own ID = update scenario — not a duplicate
            var result = await CreateQueryRepo().AlreadyExistsAsync("EXCL01", miscTypeId, id: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_ForSoftDeleted_Records()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QDUP01");
            var id = await SeedMiscMasterAsync(miscTypeId, "DELDUP01", "Item");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().AlreadyExistsAsync("DELDUP01", miscTypeId);

            result.Should().BeFalse();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QNFEXIST");
            var id = await SeedMiscMasterAsync(miscTypeId, "NF001", "Item");

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearMiscMasterTableAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QNFDEL");
            var id = await SeedMiscMasterAsync(miscTypeId, "NFDEL01", "Item");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── MiscTypeExistsAsync ───────────────────────────────────────────────

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_True_WhenActiveTypeExists()
        {
            var miscTypeId = await EnsureMiscTypeExistsAsync("QTEXIST01");

            var result = await CreateQueryRepo().MiscTypeExistsAsync(miscTypeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_False_WhenTypeDoesNotExist()
        {
            var result = await CreateQueryRepo().MiscTypeExistsAsync(999999);

            result.Should().BeFalse();
        }

        // ── AutocompleteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QACM01");
            await SeedMiscMasterAsync(miscTypeId, "ACM001", "Acme Item One", sortOrder: 1);
            await SeedMiscMasterAsync(miscTypeId, "ACM002", "Acme Item Two", sortOrder: 2);
            await SeedMiscMasterAsync(miscTypeId, "XYZ001", "XYZ Item", sortOrder: 3);

            var results = await CreateQueryRepo().AutocompleteAsync("ACM", null, CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.Code).Should().Contain(new[] { "ACM001", "ACM002" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QACM02");
            await SeedMiscMasterAsync(miscTypeId, "CODE001", "Item One");

            var results = await CreateQueryRepo().AutocompleteAsync("ZZZNOMATCH", null, CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QACM03");
            await SeedMiscMasterAsync(miscTypeId, "ACTV01", "Active Item", isActive: true);
            await SeedMiscMasterAsync(miscTypeId, "INAC01", "Inactive Item", isActive: false);

            var results = await CreateQueryRepo().AutocompleteAsync("Item", null, CancellationToken.None);

            results.Should().NotContain(r => r.Code == "INAC01");
            results.Should().Contain(r => r.Code == "ACTV01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted_Records()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId = await EnsureMiscTypeExistsAsync("QACM04");
            var id = await SeedMiscMasterAsync(miscTypeId, "DLAUTO01", "Deleted Item");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", null, CancellationToken.None);

            results.Should().NotContain(r => r.Code == "DLAUTO01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Filter_By_MiscTypeId()
        {
            await ClearMiscMasterTableAsync();
            var miscTypeId1 = await EnsureMiscTypeExistsAsync("QACMF01");
            var miscTypeId2 = await EnsureMiscTypeExistsAsync("QACMF02");
            await SeedMiscMasterAsync(miscTypeId1, "T1A01", "Type1 Item");
            await SeedMiscMasterAsync(miscTypeId2, "T2A01", "Type2 Item");

            var results = await CreateQueryRepo().AutocompleteAsync("Item", "QACMF01", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].MiscTypeId.Should().Be(miscTypeId1);
        }
    }
}
