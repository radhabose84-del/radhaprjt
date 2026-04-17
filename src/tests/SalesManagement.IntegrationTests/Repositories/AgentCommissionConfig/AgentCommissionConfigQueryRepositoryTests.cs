using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Purchase;
using Contracts.Dtos.Lookups.Sales;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Purchase;
using Contracts.Interfaces.Lookups.Sales;
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
    /// AgentCommissionConfig JOINs Sales.MiscMaster and Sales.CommissionSplit (same-module FKs)
    /// for names, so prerequisite rows are seeded via EnsurePrerequisitesAsync().
    ///
    /// IPartyLookup, IPaymentTermLookup, ICommissionSplitLookup are mocked to isolate cross-module deps.
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
            Mock<IPaymentTermLookup> paymentTermLookup = null,
            Mock<ICommissionSplitLookup> commissionSplitLookup = null)
        {
            partyLookup           ??= BuildDefaultPartyLookup();
            paymentTermLookup     ??= BuildDefaultPaymentTermLookup();
            commissionSplitLookup ??= BuildDefaultCommissionSplitLookup();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new AgentCommissionConfigQueryRepository(
                conn,
                partyLookup.Object,
                paymentTermLookup.Object,
                commissionSplitLookup.Object);
        }

        private static AgentCommissionConfigCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new AgentCommissionConfigCommandRepository(ctx);

        private static Mock<IPartyLookup> BuildDefaultPartyLookup(int agentId = 10, string agentName = "Test Agent")
        {
            var mock = new Mock<IPartyLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PartyLookupDto>
                {
                    new PartyLookupDto { Id = agentId, PartyCode = "AGT001", PartyName = agentName }
                });
            return mock;
        }

        private static Mock<IPaymentTermLookup> BuildDefaultPaymentTermLookup()
        {
            var mock = new Mock<IPaymentTermLookup>(MockBehavior.Loose);
            mock.Setup(p => p.GetAllPaymentTermAsync())
                .ReturnsAsync(new List<PaymentTermLookupDto>());
            return mock;
        }

        private static Mock<ICommissionSplitLookup> BuildDefaultCommissionSplitLookup()
        {
            var mock = new Mock<ICommissionSplitLookup>(MockBehavior.Loose);
            mock.Setup(c => c.GetAllCommissionSplitAsync())
                .ReturnsAsync(new List<CommissionSplitLookupDto>());
            return mock;
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearTablesAsync("Sales.AgentCommissionConfig");

        private async Task<(int miscMasterId, int commissionSplitId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // MiscTypeMaster (parent for MiscMaster)
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

            // MiscMaster — used for all FKs (CommissionType / CommissionBasis / ApplicableLevel / TriggerEvent / SlabType)
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

            // CommissionSplit (same-module FK)
            var split = await ctx.CommissionSplit.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.SplitCode == "ACCSP01");
            if (split == null)
            {
                split = new Domain.Entities.CommissionSplit
                {
                    SplitCode = "ACCSP01",
                    SplitName = "ACC Integration Split",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.CommissionSplit.Add(split);
                await ctx.SaveChangesAsync();
            }
            ctx.ChangeTracker.Clear();

            return (miscMaster.Id, split.Id);
        }

        private async Task<int> SeedEntityAsync(
            int miscMasterId, int commissionSplitId,
            int agentId = 10,
            decimal commissionPct = 5.0m,
            bool isActive = true)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(new Domain.Entities.AgentCommissionConfig
            {
                AgentId = agentId,
                CommissionTypeId = miscMasterId,
                CommissionBasisId = miscMasterId,
                ApplicableLevelId = miscMasterId,
                TriggerEventId = miscMasterId,
                SlabTypeId = miscMasterId,
                CommissionSplitId = commissionSplitId,
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
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            await SeedEntityAsync(miscId, splitId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(miscId, splitId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CommissionTypeName()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            await SeedEntityAsync(miscId, splitId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].CommissionTypeName.Should().Be("ACC Commission Type");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_SplitCode()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            await SeedEntityAsync(miscId, splitId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].SplitCode.Should().Be("ACCSP01");
        }

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Fields()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(miscId, splitId, agentId: 10, commissionPct: 7.5m);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.AgentId.Should().Be(10);
            dto.CommissionPercentage.Should().Be(7.5m);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(miscId, splitId);

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
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(miscId, splitId);

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
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(miscId, splitId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeTrue();
        }

        // ── MiscMasterExistsAsync ─────────────────────────────────────────────

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_For_ExistingId()
        {
            var (miscId, _) = await EnsurePrerequisitesAsync();

            var exists = await CreateQueryRepo().MiscMasterExistsAsync(miscId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_For_NonExistentId()
        {
            var exists = await CreateQueryRepo().MiscMasterExistsAsync(999999);

            exists.Should().BeFalse();
        }

        // ── CommissionSplitExistsAsync ────────────────────────────────────────

        [Fact]
        public async Task CommissionSplitExistsAsync_Should_Return_True_For_ExistingId()
        {
            var (_, splitId) = await EnsurePrerequisitesAsync();

            var exists = await CreateQueryRepo().CommissionSplitExistsAsync(splitId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CommissionSplitExistsAsync_Should_Return_False_For_NonExistentId()
        {
            var exists = await CreateQueryRepo().CommissionSplitExistsAsync(999999);

            exists.Should().BeFalse();
        }

        // ── OverlapExistsAsync ────────────────────────────────────────────────

        [Fact]
        public async Task OverlapExistsAsync_Should_Return_True_When_Overlap_Exists()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            await SeedEntityAsync(miscId, splitId, agentId: 10);

            var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to   = new DateTimeOffset(2025, 8, 31, 0, 0, 0, TimeSpan.Zero);

            var overlaps = await CreateQueryRepo().OverlapExistsAsync(10, splitId, from, to);

            overlaps.Should().BeTrue();
        }

        [Fact]
        public async Task OverlapExistsAsync_Should_Return_False_When_No_Overlap()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            await SeedEntityAsync(miscId, splitId, agentId: 10);

            var from = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var to   = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

            var overlaps = await CreateQueryRepo().OverlapExistsAsync(10, splitId, from, to);

            overlaps.Should().BeFalse();
        }

        [Fact]
        public async Task OverlapExistsAsync_Should_Exclude_Self_When_ExcludeId_Provided()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await ClearTableAsync();
            var id = await SeedEntityAsync(miscId, splitId, agentId: 10);

            var from = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var to   = new DateTimeOffset(2025, 8, 31, 0, 0, 0, TimeSpan.Zero);

            var overlaps = await CreateQueryRepo().OverlapExistsAsync(10, splitId, from, to, excludeId: id);

            overlaps.Should().BeFalse();
        }
    }
}
