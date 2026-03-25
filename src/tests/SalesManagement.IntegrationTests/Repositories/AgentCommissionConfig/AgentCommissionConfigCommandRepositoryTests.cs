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
    /// AgentCommissionConfig requires:
    ///   - Sales.SalesSegment (DB FK) → which requires SalesOrganisation, SalesChannel, BusinessUnit
    ///   - Sales.MiscMaster (DB FK) → which requires MiscTypeMaster
    ///
    /// AgentId, CurrencyId are cross-module FKs — no DB constraint, any int value is valid.
    /// CommissionBasisId, ApplicableLevelId are same-module FKs to MiscMaster (nullable).
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

        private async Task<(int salesSegmentId, int commissionTypeId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // SalesOrganisation
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

            // SalesChannel
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

            // BusinessUnit
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

            // SalesSegment
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

            // MiscTypeMaster (prerequisite for MiscMaster)
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

            // MiscMaster (CommissionType)
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

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.AgentCommissionConfig");
        }

        private static Domain.Entities.AgentCommissionConfig BuildEntity(
            int salesSegmentId,
            int commissionTypeId,
            int agentId = 10,
            decimal commissionPercentage = 5.00m,
            DateTimeOffset? validityFrom = null,
            DateTimeOffset? validityTo = null)
            => new Domain.Entities.AgentCommissionConfig
            {
                AgentId = agentId,
                SalesSegmentId = salesSegmentId,
                CommissionTypeId = commissionTypeId,
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
            var (segmentId, typeId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(segmentId, typeId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            var (segmentId, typeId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var validFrom = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
            var validTo   = new DateTimeOffset(2025, 9, 30, 0, 0, 0, TimeSpan.Zero);
            var entity = BuildEntity(segmentId, typeId,
                agentId: 20,
                commissionPercentage: 7.5m,
                validityFrom: validFrom,
                validityTo: validTo);

            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.AgentCommissionConfig.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.AgentId.Should().Be(20);
            saved.SalesSegmentId.Should().Be(segmentId);
            saved.CommissionTypeId.Should().Be(typeId);
            saved.CommissionPercentage.Should().Be(7.5m);
            saved.ValidityFrom.Should().Be(validFrom);
            saved.ValidityTo.Should().Be(validTo);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            var (segmentId, typeId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(segmentId, typeId));
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
            var (segmentId, typeId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(
                BuildEntity(segmentId, typeId, agentId: 10, commissionPercentage: 5.0m));
            ctx.ChangeTracker.Clear();

            var newFrom = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var newTo   = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
            var updated = new Domain.Entities.AgentCommissionConfig
            {
                Id = id,
                AgentId = 15,
                SalesSegmentId = segmentId,
                CommissionTypeId = typeId,
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
            await using var ctx = _fixture.CreateFreshDbContext();

            var updated = new Domain.Entities.AgentCommissionConfig
            {
                Id = 99999,
                AgentId = 1,
                SalesSegmentId = 1,
                CommissionTypeId = 1,
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
            var (segmentId, typeId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(segmentId, typeId));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.AgentCommissionConfig
            {
                Id = id,
                AgentId = 10,
                SalesSegmentId = segmentId,
                CommissionTypeId = typeId,
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
            var (segmentId, typeId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(segmentId, typeId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            var (segmentId, typeId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(segmentId, typeId));
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
            var (segmentId, typeId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(segmentId, typeId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
