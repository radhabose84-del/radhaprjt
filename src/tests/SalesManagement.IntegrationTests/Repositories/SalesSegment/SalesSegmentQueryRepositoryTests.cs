using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesSegment;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesSegment
{
    /// <summary>
    /// Integration tests for SalesSegmentQueryRepository.
    /// Verifies Dapper SQL queries against a real SQL Server database.
    ///
    /// GetAllAsync and GetByIdAsync use INNER JOINs to Sales.SalesOrganisation,
    /// Sales.SalesChannel and Sales.BusinessUnit (same-module), so prerequisite rows
    /// are seeded via EnsurePrerequisitesAsync() before each test.
    ///
    /// The unique index on (SalesOrganisationId, SalesChannelId, BusinessUnitId) means
    /// each combination can only appear once. Tests that need multiple segments use
    /// different BusinessUnit IDs from the pre-seeded pool (INTBU01..INTBU05).
    ///
    /// CurrencyName is NOT populated by the repository (handled at application layer).
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesSegmentQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesSegmentQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private SqlConnection OpenConnection() => new SqlConnection(_fixture.ConnectionString);

        private SalesSegmentQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesSegmentQueryRepository(conn);
        }

        private SalesSegmentCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new SalesSegmentCommandRepository(ctx);

        /// <summary>
        /// Seeds one SalesOrganisation, one SalesChannel, and a pool of five BusinessUnits
        /// (INTBU01–INTBU05) if they do not already exist. Idempotent.
        /// Returns (orgId, channelId, buIds[5]) so tests can use different BU IDs per segment,
        /// avoiding the unique-index violation on (OrgId, ChannelId, BuId).
        /// </summary>
        private async Task<(int orgId, int channelId, int[] buIds)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // SalesOrganisation
            var org = await ctx.SalesOrganisation
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOrganisationCode == "INTSO01");
            if (org == null)
            {
                org = new Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "INTSO01",
                    SalesOrganisationName = "Integration Test Org",
                    CompanyId = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesOrganisation.Add(org);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            // SalesChannel
            var channel = await ctx.SalesChannel
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesChannelCode == "INTSC01");
            if (channel == null)
            {
                channel = new Domain.Entities.SalesChannel
                {
                    SalesChannelCode = "INTSC01",
                    SalesChannelName = "Integration Test Channel",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesChannel.Add(channel);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            // BusinessUnit pool: INTBU01 – INTBU05
            var buIds = new int[5];
            for (int i = 1; i <= 5; i++)
            {
                var code = $"INTBU0{i}";
                var bu = await ctx.BusinessUnit
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.BusinessUnitCode == code);
                if (bu == null)
                {
                    bu = new Domain.Entities.BusinessUnit
                    {
                        BusinessUnitCode = code,
                        BusinessUnitName = $"Integration Test BU {i}",
                        Description = $"Integration Test BU {i}",
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    };
                    ctx.BusinessUnit.Add(bu);
                    await ctx.SaveChangesAsync();
                }
                ctx.ChangeTracker.Clear();
                buIds[i - 1] = bu.Id;
            }

            return (org.Id, channel.Id, buIds);
        }

        private async Task ClearSalesSegmentAsync()
        {
            await using var cnn = OpenConnection();
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesItemPriceMaster");
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesSegment");
        }

        private async Task<int> SeedEntityAsync(
            int orgId, int channelId, int buId,
            string name = "Test Segment",
            bool isActive = true,
            int? currencyId = null,
            DateTime? validFrom = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Domain.Entities.SalesSegment
            {
                SalesOrganisationId = orgId,
                SalesChannelId = channelId,
                BusinessUnitId = buId,
                SegmentName = name,
                CurrencyId = currencyId,
                ValidFrom = validFrom,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_PagedResults()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            // Each segment uses a different BU to satisfy the unique composite key index
            await SeedEntityAsync(orgId, channelId, buIds[0], "Segment Alpha");
            await SeedEntityAsync(orgId, channelId, buIds[1], "Segment Beta");
            await SeedEntityAsync(orgId, channelId, buIds[2], "Segment Gamma");

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 2, searchTerm: null);

            totalCount.Should().Be(3);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySegmentName()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            await SeedEntityAsync(orgId, channelId, buIds[0], "Alpha Segment");
            await SeedEntityAsync(orgId, channelId, buIds[1], "Beta Segment");
            await SeedEntityAsync(orgId, channelId, buIds[2], "Unique Match Segment");

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, "Unique Match");

            totalCount.Should().Be(1);
            data[0].SegmentName.Should().Be("Unique Match Segment");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            var id = await SeedEntityAsync(orgId, channelId, buIds[0], "Deleted Segment");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.SegmentName == "Deleted Segment");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, null);

            data.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination_Page2()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            for (int i = 0; i < 5; i++)
                await SeedEntityAsync(orgId, channelId, buIds[i], $"Segment {i + 1:D2}");

            var repo = CreateQueryRepo();
            var (page1, total) = await repo.GetAllAsync(1, 3, null);
            var (page2, _) = await repo.GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            var page1Names = page1.Select(x => x.SegmentName).ToList();
            var page2Names = page2.Select(x => x.SegmentName).ToList();
            page1Names.Should().NotIntersectWith(page2Names);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_RelatedNames_FromJoins()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            await SeedEntityAsync(orgId, channelId, buIds[0], "Join Test Segment");

            var repo = CreateQueryRepo();
            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotBeEmpty();
            data[0].SalesOrganisationId.Should().Be(orgId);
            data[0].SalesOrganisationName.Should().Be("Integration Test Org");
            data[0].SalesChannelId.Should().Be(channelId);
            data[0].SalesChannelName.Should().Be("Integration Test Channel");
            data[0].BusinessUnitId.Should().Be(buIds[0]);
            data[0].BusinessUnitName.Should().NotBeNullOrEmpty();
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            var validFrom = new DateTime(2025, 3, 15);
            var id = await SeedEntityAsync(orgId, channelId, buIds[0],
                name: "ById Segment", currencyId: 7, validFrom: validFrom);

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.SegmentName.Should().Be("ById Segment");
            dto.CurrencyId.Should().Be(7);
            dto.ValidFrom.Should().Be(validFrom);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_RelatedNames_FromJoins()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            var id = await SeedEntityAsync(orgId, channelId, buIds[0], "Join ById Segment");

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto!.SalesOrganisationName.Should().Be("Integration Test Org");
            dto.SalesChannelName.Should().Be("Integration Test Channel");
            dto.BusinessUnitName.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            var id = await SeedEntityAsync(orgId, channelId, buIds[0], "SoftDel Segment");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // ── CompositeKeyExistsAsync ───────────────────────────────────────────

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_WhenExists()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            await SeedEntityAsync(orgId, channelId, buIds[0]);

            var repo = CreateQueryRepo();
            var result = await repo.CompositeKeyExistsAsync(orgId, channelId, buIds[0]);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_WhenNotExists()
        {
            await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();

            var repo = CreateQueryRepo();
            var result = await repo.CompositeKeyExistsAsync(9001, 9002, 9003);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_WhenExcludedId_Matches()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            var id = await SeedEntityAsync(orgId, channelId, buIds[0]);

            // Excluding the same ID — update scenario for that record, should not be flagged as duplicate
            var repo = CreateQueryRepo();
            var result = await repo.CompositeKeyExistsAsync(orgId, channelId, buIds[0], excludeId: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_WhenAnotherRecord_HasSameKey()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();

            // Record A holds (orgId, channelId, buIds[0])
            await SeedEntityAsync(orgId, channelId, buIds[0], "Segment A");

            // Record B holds (orgId, channelId, buIds[1]) — different key, no conflict
            var idB = await SeedEntityAsync(orgId, channelId, buIds[1], "Segment B");

            // Record B tries to claim buIds[0] — that composite key already belongs to record A
            var repo = CreateQueryRepo();
            var result = await repo.CompositeKeyExistsAsync(orgId, channelId, buIds[0], excludeId: idB);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_ForDeleted_Records()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            var id = await SeedEntityAsync(orgId, channelId, buIds[0]);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            // After soft delete, that composite key combination should be free again
            var repo = CreateQueryRepo();
            var result = await repo.CompositeKeyExistsAsync(orgId, channelId, buIds[0]);

            result.Should().BeFalse();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            var id = await SeedEntityAsync(orgId, channelId, buIds[0]);

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(id);

            result.Should().BeFalse(); // false = entity IS found
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(99999);

            result.Should().BeTrue(); // true = entity is NOT found
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            var id = await SeedEntityAsync(orgId, channelId, buIds[0]);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── AutocompleteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            await SeedEntityAsync(orgId, channelId, buIds[0], "Acme Segment");
            await SeedEntityAsync(orgId, channelId, buIds[1], "Acme North");
            await SeedEntityAsync(orgId, channelId, buIds[2], "XYZ Segment");

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Acme", CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.SegmentName).Should().Contain(new[] { "Acme Segment", "Acme North" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            await SeedEntityAsync(orgId, channelId, buIds[0], "Segment One");

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            await SeedEntityAsync(orgId, channelId, buIds[0], "Active Segment", isActive: true);
            await SeedEntityAsync(orgId, channelId, buIds[1], "Inactive Segment", isActive: false);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Segment", CancellationToken.None);

            results.Should().NotContain(r => r.SegmentName == "Inactive Segment");
            results.Should().Contain(r => r.SegmentName == "Active Segment");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted_Records()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            var id = await SeedEntityAsync(orgId, channelId, buIds[0], "Deleted Auto Segment");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().NotContain(r => r.SegmentName == "Deleted Auto Segment");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_All_Active_WhenTermIsNull()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            await SeedEntityAsync(orgId, channelId, buIds[0], "Active One", isActive: true);
            await SeedEntityAsync(orgId, channelId, buIds[1], "Active Two", isActive: true);
            await SeedEntityAsync(orgId, channelId, buIds[2], "Inactive One", isActive: false);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync(null, CancellationToken.None);

            results.Should().HaveCount(2);
            results.Should().NotContain(r => r.SegmentName == "Inactive One");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_CompositeKeyIds()
        {
            var (orgId, channelId, buIds) = await EnsurePrerequisitesAsync();
            await ClearSalesSegmentAsync();
            await SeedEntityAsync(orgId, channelId, buIds[0], "Key Test Segment");

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("Key Test", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].SalesOrganisationId.Should().Be(orgId);
            results[0].SalesChannelId.Should().Be(channelId);
            results[0].BusinessUnitId.Should().Be(buIds[0]);
        }

        // ── FK Existence Methods ──────────────────────────────────────────────

        [Fact]
        public async Task SalesOrganisationExistsAsync_Should_Return_True_WhenExists()
        {
            var (orgId, _, _) = await EnsurePrerequisitesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.SalesOrganisationExistsAsync(orgId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesOrganisationExistsAsync_Should_Return_False_WhenNotExists()
        {
            var repo = CreateQueryRepo();
            var result = await repo.SalesOrganisationExistsAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SalesChannelExistsAsync_Should_Return_True_WhenExists()
        {
            var (_, channelId, _) = await EnsurePrerequisitesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.SalesChannelExistsAsync(channelId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesChannelExistsAsync_Should_Return_False_WhenNotExists()
        {
            var repo = CreateQueryRepo();
            var result = await repo.SalesChannelExistsAsync(99999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task BusinessUnitExistsAsync_Should_Return_True_WhenExists()
        {
            var (_, _, buIds) = await EnsurePrerequisitesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.BusinessUnitExistsAsync(buIds[0]);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task BusinessUnitExistsAsync_Should_Return_False_WhenNotExists()
        {
            var repo = CreateQueryRepo();
            var result = await repo.BusinessUnitExistsAsync(99999);

            result.Should().BeFalse();
        }
    }
}
