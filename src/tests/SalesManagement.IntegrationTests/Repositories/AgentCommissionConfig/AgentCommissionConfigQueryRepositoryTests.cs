using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.AgentCommissionConfig;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.AgentCommissionConfig
{
    /// <summary>
    /// Integration tests for AgentCommissionConfigQueryRepository.
    /// Verifies Dapper SQL queries against a real SQL Server database.
    ///
    /// AgentCommissionConfig JOINs Sales.SalesSegment and Sales.MiscMaster (same-module FKs) for names,
    /// so prerequisite rows are seeded via EnsurePrerequisitesAsync().
    ///
    /// IPartyLookup, ICurrencyLookup are mocked to isolate cross-module deps.
    /// Tests verify SQL query correctness, pagination, soft-delete exclusion, and FK validation.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class AgentCommissionConfigQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AgentCommissionConfigQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private AgentCommissionConfigQueryRepository CreateQueryRepo(
            Mock<IPartyLookup> partyLookup = null,
            Mock<ICurrencyLookup> currencyLookup = null)
        {
            partyLookup    ??= BuildDefaultPartyLookup();
            currencyLookup ??= BuildDefaultCurrencyLookup();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AgentCommissionConfigQueryRepository(
                conn,
                partyLookup.Object,
                currencyLookup.Object);
        }

        private AgentCommissionConfigCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new AgentCommissionConfigCommandRepository(ctx);

        private Mock<IPartyLookup> BuildDefaultPartyLookup(int agentId = 10, string agentName = "Test Agent")
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto>
                {
                    new PartyLookupDto { Id = agentId, PartyCode = "AGT001", PartyName = agentName }
                });
            return mock;
        }

        private Mock<ICurrencyLookup> BuildDefaultCurrencyLookup()
        {
            var mock = new Mock<ICurrencyLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>());
            return mock;
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.AgentCommissionConfig");
        }

        private async Task<(int salesSegmentId, int commissionTypeId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var org = await ctx.SalesOrganisation.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesOrganisationCode == "ACCSO01");
            if (org == null)
            {
                org = new Domain.Entities.SalesOrganisation
                {
                    SalesOrganisationCode = "ACCSO01",
                    SalesOrganisationName = "ACC Integration Org",
                    CompanyId = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesOrganisation.Add(org);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            var channel = await ctx.SalesChannel.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SalesChannelCode == "ACCSC01");
            if (channel == null)
            {
                channel = new Domain.Entities.SalesChannel
                {
                    SalesChannelCode = "ACCSC01",
                    SalesChannelName = "ACC Integration Channel",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesChannel.Add(channel);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            var bu = await ctx.BusinessUnit.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.BusinessUnitCode == "ACCBU01");
            if (bu == null)
            {
                bu = new Domain.Entities.BusinessUnit
                {
                    BusinessUnitCode = "ACCBU01",
                    BusinessUnitName = "ACC Integration BU",
                    Description = "ACC Integration BU",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
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
                    SalesOrganisationId = org.Id,
                    SalesChannelId = channel.Id,
                    BusinessUnitId = bu.Id,
                    SegmentName = "ACC Integration Segment",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesSegment.Add(segment);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            var miscType = await ctx.MiscTypeMaster.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.MiscTypeCode == "ACCMT01");
            if (miscType == null)
            {
                miscType = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ACCMT01",
                    Description = "ACC Commission Types",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscTypeMaster.Add(miscType);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            var miscMaster = await ctx.MiscMaster.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Code == "ACCCM01");
            if (miscMaster == null)
            {
                miscMaster = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id,
                    Code = "ACCCM01",
                    Description = "ACC Commission Type",
                    SortOrder = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscMaster.Add(miscMaster);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            return (segment.Id, miscMaster.Id);
        }

        private async Task<int> SeedEntityAsync(
            int salesSegmentId, int commissionTypeId,
            int agentId = 10,
            decimal commissionPct = 5.0m,
            bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(new Domain.Entities.AgentCommissionConfig
            {
                AgentId = agentId,
                SalesSegmentId = salesSegmentId,
                CommissionTypeId = commissionTypeId,
                CommissionPercentage = commissionPct,
                ValidityFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ValidityTo = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Records()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            await SeedEntityAsync(segId, typeId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(segId, typeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_SegmentName()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            await SeedEntityAsync(segId, typeId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].SegmentName.Should().Be("ACC Integration Segment");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CommissionTypeName()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            await SeedEntityAsync(segId, typeId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].CommissionTypeName.Should().Be("ACC Commission Type");
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Fields()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(segId, typeId, agentId: 10, commissionPct: 7.5m);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.AgentId.Should().Be(10);
            dto.CommissionPercentage.Should().Be(7.5m);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(segId, typeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_For_NonExistentId()
        {
            var dto = await CreateQueryRepo().GetByIdAsync(999999);

            dto.Should().BeNull();
        }

        // ── NotFoundAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(segId, typeId);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Not_Exists()
        {
            var notFound = await CreateQueryRepo().NotFoundAsync(999999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_SoftDeleted()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(segId, typeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeTrue();
        }

        // ── SalesSegmentExistsAsync ───────────────────────────────────────────

        [Fact]
        public async Task SalesSegmentExistsAsync_Should_Return_True_For_ExistingSegment()
        {
            var (segId, _) = await EnsurePrerequisitesAsync();

            var exists = await CreateQueryRepo().SalesSegmentExistsAsync(segId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task SalesSegmentExistsAsync_Should_Return_False_For_NonExistentSegment()
        {
            var exists = await CreateQueryRepo().SalesSegmentExistsAsync(999999);

            exists.Should().BeFalse();
        }

        // ── CommissionTypeExistsAsync ─────────────────────────────────────────

        [Fact]
        public async Task CommissionTypeExistsAsync_Should_Return_True_For_ExistingType()
        {
            var (_, typeId) = await EnsurePrerequisitesAsync();

            var exists = await CreateQueryRepo().CommissionTypeExistsAsync(typeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CommissionTypeExistsAsync_Should_Return_False_For_NonExistentType()
        {
            var exists = await CreateQueryRepo().CommissionTypeExistsAsync(999999);

            exists.Should().BeFalse();
        }

        // ── OverlapExistsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task OverlapExistsAsync_Should_Return_True_When_Overlap_Exists()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            await SeedEntityAsync(segId, typeId, agentId: 10);

            var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to   = new DateTimeOffset(2025, 8, 31, 0, 0, 0, TimeSpan.Zero);

            var overlaps = await CreateQueryRepo().OverlapExistsAsync(10, segId, from, to);

            overlaps.Should().BeTrue();
        }

        [Fact]
        public async Task OverlapExistsAsync_Should_Return_False_When_No_Overlap()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            await SeedEntityAsync(segId, typeId, agentId: 10);

            var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var to   = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

            var overlaps = await CreateQueryRepo().OverlapExistsAsync(10, segId, from, to);

            overlaps.Should().BeFalse();
        }

        [Fact]
        public async Task OverlapExistsAsync_Should_Exclude_Self_When_ExcludeId_Provided()
        {
            var (segId, typeId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(segId, typeId, agentId: 10);

            var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to   = new DateTimeOffset(2025, 8, 31, 0, 0, 0, TimeSpan.Zero);

            var overlaps = await CreateQueryRepo().OverlapExistsAsync(10, segId, from, to, excludeId: id);

            overlaps.Should().BeFalse();
        }
    }
}
