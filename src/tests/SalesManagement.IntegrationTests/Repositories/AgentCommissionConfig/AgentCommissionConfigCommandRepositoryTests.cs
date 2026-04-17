using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.AgentCommissionConfig;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.AgentCommissionConfig
{
    /// <summary>
    /// Integration tests for AgentCommissionConfigCommandRepository.
    /// Verifies EF Core Create, Update, and SoftDelete operations against a real SQL Server database.
    ///
    /// AgentCommissionConfig requires same-module FKs (DB constraints):
    ///   - Sales.MiscMaster (CommissionTypeId, CommissionBasisId, ApplicableLevelId, TriggerEventId, optional SlabTypeId)
    ///       → MiscMaster requires Sales.MiscTypeMaster
    ///   - Sales.CommissionSplit (CommissionSplitId)
    ///
    /// AgentId is a cross-module FK — no DB constraint, any int value is valid.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class AgentCommissionConfigCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public AgentCommissionConfigCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private AgentCommissionConfigCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new AgentCommissionConfigCommandRepository(ctx);

        // ── Prerequisites ─────────────────────────────────────────────────────

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

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearTablesAsync("Sales.AgentCommissionConfig");

        private static Domain.Entities.AgentCommissionConfig BuildEntity(
            int miscMasterId,
            int commissionSplitId,
            int agentId = 10,
            decimal commissionPercentage = 5.00m,
            DateTimeOffset? validityFrom = null,
            DateTimeOffset? validityTo = null)
            => new Domain.Entities.AgentCommissionConfig
            {
                AgentId = agentId,
                CommissionTypeId = miscMasterId,
                CommissionBasisId = miscMasterId,
                ApplicableLevelId = miscMasterId,
                TriggerEventId = miscMasterId,
                SlabTypeId = miscMasterId,
                CommissionSplitId = commissionSplitId,
                CommissionPercentage = commissionPercentage,
                ValidityFrom = validityFrom ?? new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ValidityTo = validityTo ?? new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, splitId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var validFrom = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
            var validTo   = new DateTimeOffset(2025, 9, 30, 0, 0, 0, TimeSpan.Zero);
            var entity = BuildEntity(miscId, splitId,
                agentId: 20,
                commissionPercentage: 7.5m,
                validityFrom: validFrom,
                validityTo: validTo);

            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AgentCommissionConfig.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.AgentId.Should().Be(20);
            saved.CommissionTypeId.Should().Be(miscId);
            saved.CommissionSplitId.Should().Be(splitId);
            saved.CommissionPercentage.Should().Be(7.5m);
            saved.ValidityFrom.Should().Be(validFrom);
            saved.ValidityTo.Should().Be(validTo);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, splitId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AgentCommissionConfig.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Update_Mutable_Fields()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(
                BuildEntity(miscId, splitId, agentId: 10, commissionPercentage: 5.0m));
            ctx.ChangeTracker.Clear();

            var newFrom = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var newTo   = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
            var updated = new Domain.Entities.AgentCommissionConfig
            {
                Id = id,
                AgentId = 15,
                CommissionTypeId = miscId,
                CommissionBasisId = miscId,
                ApplicableLevelId = miscId,
                TriggerEventId = miscId,
                SlabTypeId = miscId,
                CommissionSplitId = splitId,
                CommissionPercentage = 10.0m,
                ValidityFrom = newFrom,
                ValidityTo = newTo,
                IsActive = Status.Inactive
            };

            var resultId = await CreateRepository(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);

            var saved = await ctx.AgentCommissionConfig.FirstOrDefaultAsync(x => x.Id == id);
            saved!.AgentId.Should().Be(15);
            saved.CommissionPercentage.Should().Be(10.0m);
            saved.ValidityFrom.Should().Be(newFrom);
            saved.ValidityTo.Should().Be(newTo);
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var updated = new Domain.Entities.AgentCommissionConfig
            {
                Id = 99999,
                AgentId = 1,
                CommissionTypeId = miscId,
                CommissionBasisId = miscId,
                ApplicableLevelId = miscId,
                TriggerEventId = miscId,
                CommissionSplitId = splitId,
                CommissionPercentage = 5.0m,
                ValidityFrom = DateTimeOffset.UtcNow,
                ValidityTo = DateTimeOffset.UtcNow.AddMonths(6),
                IsActive = Status.Active
            };

            var resultId = await CreateRepository(ctx).UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_ModifiedAuditFields()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, splitId));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.AgentCommissionConfig
            {
                Id = id,
                AgentId = 10,
                CommissionTypeId = miscId,
                CommissionBasisId = miscId,
                ApplicableLevelId = miscId,
                TriggerEventId = miscId,
                SlabTypeId = miscId,
                CommissionSplitId = splitId,
                CommissionPercentage = 8.0m,
                ValidityFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ValidityTo = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                IsActive = Status.Active
            };

            await CreateRepository(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AgentCommissionConfig.FirstOrDefaultAsync(x => x.Id == id);

            saved!.ModifiedBy.Should().Be(1);
            saved.ModifiedByName.Should().Be("test-user");
            saved.ModifiedIP.Should().Be("127.0.0.1");
            saved.ModifiedDate.Should().NotBeNull();
        }

        // ── SoftDeleteAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, splitId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, splitId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AgentCommissionConfig
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            var (miscId, splitId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(miscId, splitId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
