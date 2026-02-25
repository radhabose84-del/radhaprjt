#nullable disable
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesItemPriceMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesItemPriceMaster
{
    /// <summary>
    /// Integration tests for SalesItemPriceMasterQueryRepository.
    /// Verifies Dapper SQL queries against a real SQL Server database.
    ///
    /// SalesItemPriceMaster JOINs Sales.SalesSegment (same-module FK) for SegmentName,
    /// so prerequisite SalesSegment rows are seeded via EnsurePrerequisitesAsync().
    ///
    /// IItemLookup, ICurrencyLookup, IPaymentTermLookup are mocked to isolate
    /// cross-module dependencies. Tests verify SQL query correctness, not lookup enrichment.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesItemPriceMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesItemPriceMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private SqlConnection OpenConnection() => new SqlConnection(_fixture.ConnectionString);

        private SalesItemPriceMasterQueryRepository CreateQueryRepo(
            Mock<IItemLookup> itemLookup = null,
            Mock<ICurrencyLookup> currencyLookup = null,
            Mock<IPaymentTermLookup> paymentTermLookup = null)
        {
            itemLookup       ??= BuildDefaultItemLookup();
            currencyLookup   ??= BuildDefaultCurrencyLookup();
            paymentTermLookup ??= BuildDefaultPaymentTermLookup();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new SalesItemPriceMasterQueryRepository(
                conn,
                itemLookup.Object,
                currencyLookup.Object,
                paymentTermLookup.Object);
        }

        private SalesItemPriceMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new SalesItemPriceMasterCommandRepository(ctx);

        private Mock<IItemLookup> BuildDefaultItemLookup(int itemId = 100)
        {
            var mock = new Mock<IItemLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto>
                {
                    new ItemLookupDto { Id = itemId, ItemCode = "ITEM001", ItemName = "Test Item" }
                });
            return mock;
        }

        private Mock<ICurrencyLookup> BuildDefaultCurrencyLookup(int currencyId = 5)
        {
            var mock = new Mock<ICurrencyLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>
                {
                    new CurrencyLookupDto { CurrencyId = currencyId, Code = "USD", Name = "US Dollar" }
                });
            return mock;
        }

        private Mock<IPaymentTermLookup> BuildDefaultPaymentTermLookup(int ptId = 10)
        {
            var mock = new Mock<IPaymentTermLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllPaymentTermAsync())
                .ReturnsAsync(new List<PaymentTermLookupDto>
                {
                    new PaymentTermLookupDto { Id = ptId, Code = "NET30", Description = "Net 30 Days" }
                });
            return mock;
        }

        /// <summary>Seeds all prerequisites: org → channel → BU → segment. Idempotent.</summary>
        private async Task<int> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var org = await ctx.SalesOrganisation.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOrganisationCode == "INTSO01");
            if (org == null)
            {
                org = new Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "INTSO01", SalesOrganisationName = "Integration Test Org",
                    CompanyId = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesOrganisation.Add(org);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            var channel = await ctx.SalesChannel.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesChannelCode == "INTSC01");
            if (channel == null)
            {
                channel = new Domain.Entities.SalesChannel
                {
                    SalesChannelCode = "INTSC01", SalesChannelName = "Integration Test Channel",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesChannel.Add(channel);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            var bu = await ctx.BusinessUnit.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.BusinessUnitCode == "INTBU01");
            if (bu == null)
            {
                bu = new Domain.Entities.BusinessUnit
                {
                    BusinessUnitCode = "INTBU01", BusinessUnitName = "Integration Test BU",
                    Description = "Integration Test BU",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                ctx.BusinessUnit.Add(bu);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            var segment = await ctx.SalesSegment.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x =>
                    x.SalesOrganisationId == org.Id &&
                    x.SalesChannelId == channel.Id &&
                    x.BusinessUnitId == bu.Id);
            if (segment == null)
            {
                segment = new Domain.Entities.SalesSegment
                {
                    SalesOrganisationId = org.Id, SalesChannelId = channel.Id, BusinessUnitId = bu.Id,
                    SegmentName = "Integration Test Segment", IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesSegment.Add(segment);
                await ctx.SaveChangesAsync();
            }

            return segment.Id;
        }

        private async Task ClearPriceMasterAsync()
        {
            await using var cnn = OpenConnection();
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("DELETE FROM Sales.SalesItemPriceMaster");
        }

        private async Task<int> SeedEntityAsync(
            int salesSegmentId,
            string priceCode,
            int itemId = 100,
            int paymentTermsId = 10,
            decimal exMillPrice = 250.00m,
            int currencyId = 5,
            bool isActive = true,
            DateTimeOffset? validFrom = null,
            DateTimeOffset? validTo = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Domain.Entities.SalesItemPriceMaster
            {
                PriceCode      = priceCode,
                ItemId         = itemId,
                SalesSegmentId = salesSegmentId,
                PaymentTermsId = paymentTermsId,
                ExMillPrice    = exMillPrice,
                CurrencyId     = currencyId,
                ValidFrom      = validFrom ?? new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ValidTo        = validTo   ?? new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                IsActive       = isActive ? Status.Active : Status.Inactive,
                IsDeleted      = IsDelete.NotDeleted
            };
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_PagedResults()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            await SeedEntityAsync(segmentId, "PC001");
            await SeedEntityAsync(segmentId, "PC002");
            await SeedEntityAsync(segmentId, "PC003");

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(pageNumber: 1, pageSize: 2, searchTerm: null);

            totalCount.Should().Be(3);
            data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_ByPriceCode()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            await SeedEntityAsync(segmentId, "PC001");
            await SeedEntityAsync(segmentId, "PC002");
            await SeedEntityAsync(segmentId, "SPECIAL99");

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, "SPECIAL");

            totalCount.Should().Be(1);
            data[0].PriceCode.Should().Be("SPECIAL99");
        }

        [Fact]
        public async Task GetAllAsync_Should_Not_Return_SoftDeleted_Records()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            var id = await SeedEntityAsync(segmentId, "PC001");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotContain(x => x.PriceCode == "PC001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();

            var repo = CreateQueryRepo();
            var (data, totalCount) = await repo.GetAllAsync(1, 10, null);

            data.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination_Page2()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            for (int i = 1; i <= 5; i++)
                await SeedEntityAsync(segmentId, $"PC{i:D3}");

            var repo = CreateQueryRepo();
            var (page1, total) = await repo.GetAllAsync(1, 3, null);
            var (page2, _) = await repo.GetAllAsync(2, 3, null);

            total.Should().Be(5);
            page1.Should().HaveCount(3);
            page2.Should().HaveCount(2);

            var page1Codes = page1.Select(x => x.PriceCode).ToList();
            var page2Codes = page2.Select(x => x.PriceCode).ToList();
            page1Codes.Should().NotIntersectWith(page2Codes);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_SalesSegmentName_From_Join()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            await SeedEntityAsync(segmentId, "PC001");

            var repo = CreateQueryRepo();
            var (data, _) = await repo.GetAllAsync(1, 10, null);

            data.Should().NotBeEmpty();
            data[0].SalesSegmentId.Should().Be(segmentId);
            data[0].SalesSegmentName.Should().Be("Integration Test Segment");
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_DTO()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            var id = await SeedEntityAsync(segmentId, "PC001",
                itemId: 200, paymentTermsId: 15, exMillPrice: 750.00m, currencyId: 8);

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.PriceCode.Should().Be("PC001");
            dto.ItemId.Should().Be(200);
            dto.PaymentTermsId.Should().Be(15);
            dto.ExMillPrice.Should().Be(750.00m);
            dto.CurrencyId.Should().Be(8);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenSoftDeleted()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            var id = await SeedEntityAsync(segmentId, "PC001");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Populate_SalesSegmentName_From_Join()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            var id = await SeedEntityAsync(segmentId, "PC001");

            var repo = CreateQueryRepo();
            var dto = await repo.GetByIdAsync(id);

            dto!.SalesSegmentName.Should().Be("Integration Test Segment");
        }

        // ── AlreadyExistsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenExists()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            await SeedEntityAsync(segmentId, "PC001");

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("PC001");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenNotExists()
        {
            await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("ZZZNOMATCH");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_ExcludingOwnId()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            var id = await SeedEntityAsync(segmentId, "PC001");

            var repo = CreateQueryRepo();
            var result = await repo.AlreadyExistsAsync("PC001", id: id);

            result.Should().BeFalse();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            var id = await SeedEntityAsync(segmentId, "PC001");

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            var id = await SeedEntityAsync(segmentId, "PC001");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var result = await repo.NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── SalesSegmentExistsAsync ───────────────────────────────────────────

        [Fact]
        public async Task SalesSegmentExistsAsync_Should_Return_True_WhenExists()
        {
            var segmentId = await EnsurePrerequisitesAsync();

            var repo = CreateQueryRepo();
            var result = await repo.SalesSegmentExistsAsync(segmentId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SalesSegmentExistsAsync_Should_Return_False_WhenNotExists()
        {
            var repo = CreateQueryRepo();
            var result = await repo.SalesSegmentExistsAsync(99999);

            result.Should().BeFalse();
        }

        // ── AutocompleteAsync ─────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Results()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            await SeedEntityAsync(segmentId, "ACME01");
            await SeedEntityAsync(segmentId, "ACME02");
            await SeedEntityAsync(segmentId, "XYZ001");

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ACME", CancellationToken.None);

            results.Should().HaveCount(2);
            results.Select(r => r.PriceCode).Should().Contain(new[] { "ACME01", "ACME02" });
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_WhenNoMatch()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            await SeedEntityAsync(segmentId, "PC001");

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("ZZZNOMATCH", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_Inactive_Records()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            await SeedEntityAsync(segmentId, "ACTIVE01", isActive: true);
            await SeedEntityAsync(segmentId, "INACTIVE01", isActive: false);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("01", CancellationToken.None);

            results.Should().NotContain(r => r.PriceCode == "INACTIVE01");
            results.Should().Contain(r => r.PriceCode == "ACTIVE01");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Not_Return_SoftDeleted_Records()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();
            var id = await SeedEntityAsync(segmentId, "DELETED01");

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var repo = CreateQueryRepo();
            var results = await repo.AutocompleteAsync("DELETED", CancellationToken.None);

            results.Should().NotContain(r => r.PriceCode == "DELETED01");
        }

        // ── OverlapExistsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task OverlapExistsAsync_Should_Return_True_WhenOverlapExists()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();

            var validFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var validTo   = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
            await SeedEntityAsync(segmentId, "PC001", itemId: 100, paymentTermsId: 10,
                validFrom: validFrom, validTo: validTo);

            var repo = CreateQueryRepo();
            // Overlapping range
            var overlapFrom = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var overlapTo   = new DateTimeOffset(2026, 3, 31, 0, 0, 0, TimeSpan.Zero);
            var result = await repo.OverlapExistsAsync(100, segmentId, 10, overlapFrom, overlapTo);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task OverlapExistsAsync_Should_Return_False_WhenNoOverlap()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();

            var validFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var validTo   = new DateTimeOffset(2025, 6, 30, 0, 0, 0, TimeSpan.Zero);
            await SeedEntityAsync(segmentId, "PC001", itemId: 100, paymentTermsId: 10,
                validFrom: validFrom, validTo: validTo);

            var repo = CreateQueryRepo();
            // Non-overlapping range (starts after existing record ends)
            var noOverlapFrom = new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero);
            var noOverlapTo   = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
            var result = await repo.OverlapExistsAsync(100, segmentId, 10, noOverlapFrom, noOverlapTo);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task OverlapExistsAsync_Should_Return_False_When_ExcludingOwnId()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();

            var validFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var validTo   = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
            var id = await SeedEntityAsync(segmentId, "PC001", itemId: 100, paymentTermsId: 10,
                validFrom: validFrom, validTo: validTo);

            // Same range but excludeId = own Id — update scenario, should NOT flag as overlap
            var repo = CreateQueryRepo();
            var result = await repo.OverlapExistsAsync(100, segmentId, 10, validFrom, validTo, excludeId: id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task OverlapExistsAsync_Should_Return_False_ForDifferentItem()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await ClearPriceMasterAsync();

            var validFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var validTo   = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
            await SeedEntityAsync(segmentId, "PC001", itemId: 100, paymentTermsId: 10,
                validFrom: validFrom, validTo: validTo);

            // Different ItemId — should not overlap
            var repo = CreateQueryRepo();
            var result = await repo.OverlapExistsAsync(999, segmentId, 10, validFrom, validTo);

            result.Should().BeFalse();
        }
    }
}
