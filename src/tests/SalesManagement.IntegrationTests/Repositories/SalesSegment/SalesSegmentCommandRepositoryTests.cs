using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesSegment;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesSegment
{
    /// <summary>
    /// Integration tests for SalesSegmentCommandRepository.
    /// Verifies EF Core Create, Update, and SoftDelete operations against a real SQL Server database.
    ///
    /// SalesSegment requires real rows in Sales.SalesOrganisation, Sales.SalesChannel, and
    /// Sales.BusinessUnit due to same-module FK constraints.
    /// EnsurePrerequisitesAsync() seeds a fixed set of prerequisite records before each test.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesSegmentCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesSegmentCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesSegmentCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new SalesSegmentCommandRepository(ctx);

        // ── Prerequisites ─────────────────────────────────────────────────────
        // Seeds one SalesOrganisation, SalesChannel, and BusinessUnit with fixed codes.
        // Idempotent: safe to call before every test regardless of execution order.

        private async Task<(int orgId, int channelId, int buId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

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

            var bu = await ctx.BusinessUnit
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.BusinessUnitCode == "INTBU01");
            if (bu == null)
            {
                bu = new Domain.Entities.BusinessUnit
                {
                    BusinessUnitCode = "INTBU01",
                    BusinessUnitName = "Integration Test BU",
                    Description = "Integration Test BU",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.BusinessUnit.Add(bu);
                await ctx.SaveChangesAsync();
            }

            return (org.Id, channel.Id, bu.Id);
        }

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearTablesAsync("Sales.SalesSegment");

        private Domain.Entities.SalesSegment BuildEntity(
            int orgId, int channelId, int buId,
            string name = "Test Segment",
            int? currencyId = null,
            DateTime? validFrom = null)
            => new Domain.Entities.SalesSegment
            {
                SalesOrganisationId = orgId,
                SalesChannelId = channelId,
                BusinessUnitId = buId,
                SegmentName = name,
                CurrencyId = currencyId,
                ValidFrom = validFrom,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var entity = BuildEntity(orgId, channelId, buId);

            var newId = await repo.CreateAsync(entity);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Entity_With_All_Fields()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var validFrom = new DateTime(2025, 1, 1);
            var entity = BuildEntity(orgId, channelId, buId,
                name: "Alpha Segment", currencyId: 5, validFrom: validFrom);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesSegment.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.SalesOrganisationId.Should().Be(orgId);
            saved.SalesChannelId.Should().Be(channelId);
            saved.BusinessUnitId.Should().Be(buId);
            saved.SegmentName.Should().Be("Alpha Segment");
            saved.CurrencyId.Should().Be(5);
            saved.ValidFrom.Should().Be(validFrom);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Entity_With_Null_Optional_Fields()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity(orgId, channelId, buId, currencyId: null, validFrom: null);
            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesSegment.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CurrencyId.Should().BeNull();
            saved.ValidFrom.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(orgId, channelId, buId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesSegment.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Update_Mutable_Fields()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId, channelId, buId,
                name: "Original Segment", currencyId: 1, validFrom: new DateTime(2024, 1, 1)));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.SalesSegment
            {
                Id = id,
                SalesOrganisationId = orgId,
                SalesChannelId = channelId,
                BusinessUnitId = buId,
                SegmentName = "Updated Segment",
                CurrencyId = 2,
                ValidFrom = new DateTime(2025, 6, 1),
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);

            var saved = await ctx.SalesSegment.FirstOrDefaultAsync(x => x.Id == id);
            saved!.SegmentName.Should().Be("Updated Segment");
            saved.CurrencyId.Should().Be(2);
            saved.ValidFrom.Should().Be(new DateTime(2025, 6, 1));
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_CompositeKey()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId, channelId, buId, name: "Immutable Test"));
            ctx.ChangeTracker.Clear();

            // Attempt to update with different composite key values — these must be ignored
            var updated = new Domain.Entities.SalesSegment
            {
                Id = id,
                SalesOrganisationId = 9999,  // should NOT change
                SalesChannelId = 9999,        // should NOT change
                BusinessUnitId = 9999,        // should NOT change
                SegmentName = "Updated Name",
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesSegment.FirstOrDefaultAsync(x => x.Id == id);
            saved!.SalesOrganisationId.Should().Be(orgId);
            saved.SalesChannelId.Should().Be(channelId);
            saved.BusinessUnitId.Should().Be(buId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var updated = new Domain.Entities.SalesSegment
            {
                Id = 99999,
                SegmentName = "Ghost Segment",
                IsActive = Status.Active
            };

            var resultId = await repo.UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_ModifiedAuditFields()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId, channelId, buId));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.SalesSegment
            {
                Id = id,
                SegmentName = "Updated Segment",
                IsActive = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesSegment.FirstOrDefaultAsync(x => x.Id == id);

            saved!.ModifiedBy.Should().Be(1);
            saved.ModifiedByName.Should().Be("test-user");
            saved.ModifiedIP.Should().Be("127.0.0.1");
            saved.ModifiedDate.Should().NotBeNull();
        }

        // ── SoftDeleteAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId, channelId, buId));
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId, channelId, buId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesSegment
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);

            var result = await repo.SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            var (orgId, channelId, buId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(orgId, channelId, buId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
