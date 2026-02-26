using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesItemPriceMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesItemPriceMaster
{
    /// <summary>
    /// Integration tests for SalesItemPriceMasterCommandRepository.
    /// Verifies EF Core Create, Update, and SoftDelete operations against a real SQL Server database.
    ///
    /// SalesItemPriceMaster requires a real row in Sales.SalesSegment (DB FK constraint).
    /// SalesSegment in turn requires SalesOrganisation, SalesChannel, and BusinessUnit.
    /// EnsurePrerequisitesAsync() seeds all required prerequisite records before each test.
    ///
    /// ItemId, PaymentTermsId, CurrencyId are cross-module FKs — no DB FK constraint, any int value is valid.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class SalesItemPriceMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesItemPriceMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesItemPriceMasterCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new SalesItemPriceMasterCommandRepository(ctx);

        // ── Prerequisites ─────────────────────────────────────────────────────

        private async Task<int> EnsurePrerequisitesAsync()
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

            // BusinessUnit
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
            ctx.ChangeTracker.Clear();

            // SalesSegment
            var segment = await ctx.SalesSegment
                .IgnoreQueryFilters()
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
                    SegmentName = "Integration Test Segment",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.SalesSegment.Add(segment);
                await ctx.SaveChangesAsync();
            }

            return segment.Id;
        }

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.SalesItemPriceMaster");
        }

        private Domain.Entities.SalesItemPriceMaster BuildEntity(
            int salesSegmentId,
            string priceCode = "INTPC001",
            int itemId = 100,
            int paymentTermsId = 10,
            decimal exMillPrice = 250.00m,
            int currencyId = 5,
            DateTimeOffset? validFrom = null,
            DateTimeOffset? validTo = null,
            bool isActive = true)
            => new Domain.Entities.SalesItemPriceMaster
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

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(segmentId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Entity_With_All_Fields()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var validFrom = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
            var validTo   = new DateTimeOffset(2025, 9, 30, 0, 0, 0, TimeSpan.Zero);
            var entity = BuildEntity(segmentId,
                priceCode: "INTPC001", itemId: 200, paymentTermsId: 15,
                exMillPrice: 350.5000m, currencyId: 7,
                validFrom: validFrom, validTo: validTo);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesItemPriceMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.PriceCode.Should().Be("INTPC001");
            saved.ItemId.Should().Be(200);
            saved.SalesSegmentId.Should().Be(segmentId);
            saved.PaymentTermsId.Should().Be(15);
            saved.ExMillPrice.Should().Be(350.5000m);
            saved.CurrencyId.Should().Be(7);
            saved.ValidFrom.Should().Be(validFrom);
            saved.ValidTo.Should().Be(validTo);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var newId = await repo.CreateAsync(BuildEntity(segmentId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesItemPriceMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Update_Mutable_Fields()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(segmentId,
                itemId: 100, paymentTermsId: 10, exMillPrice: 100.00m, currencyId: 5));
            ctx.ChangeTracker.Clear();

            var newValidFrom = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
            var newValidTo   = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);
            var updated = new Domain.Entities.SalesItemPriceMaster
            {
                Id             = id,
                ItemId         = 999,
                SalesSegmentId = segmentId,
                PaymentTermsId = 20,
                ExMillPrice    = 500.00m,
                CurrencyId     = 8,
                ValidFrom      = newValidFrom,
                ValidTo        = newValidTo,
                IsActive       = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);

            var saved = await ctx.SalesItemPriceMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ItemId.Should().Be(999);
            saved.SalesSegmentId.Should().Be(segmentId);
            saved.PaymentTermsId.Should().Be(20);
            saved.ExMillPrice.Should().Be(500.00m);
            saved.CurrencyId.Should().Be(8);
            saved.ValidFrom.Should().Be(newValidFrom);
            saved.ValidTo.Should().Be(newValidTo);
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_PriceCode()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(segmentId, priceCode: "INTPC001"));
            ctx.ChangeTracker.Clear();

            // Attempt to update (PriceCode is immutable — not modified by UpdateAsync)
            var updated = new Domain.Entities.SalesItemPriceMaster
            {
                Id             = id,
                PriceCode      = "CHANGED",  // should be ignored by UpdateAsync
                ItemId         = 100,
                SalesSegmentId = segmentId,
                PaymentTermsId = 10,
                ExMillPrice    = 100.00m,
                CurrencyId     = 5,
                ValidFrom      = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ValidTo        = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                IsActive       = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesItemPriceMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.PriceCode.Should().Be("INTPC001");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var updated = new Domain.Entities.SalesItemPriceMaster
            {
                Id             = 99999,
                ItemId         = 1,
                SalesSegmentId = 1,
                PaymentTermsId = 1,
                ExMillPrice    = 100.00m,
                CurrencyId     = 1,
                ValidFrom      = DateTimeOffset.UtcNow,
                ValidTo        = DateTimeOffset.UtcNow.AddMonths(6),
                IsActive       = Status.Active
            };

            var resultId = await repo.UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_ModifiedAuditFields()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(segmentId));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.SalesItemPriceMaster
            {
                Id             = id,
                ItemId         = 100,
                SalesSegmentId = segmentId,
                PaymentTermsId = 10,
                ExMillPrice    = 200.00m,
                CurrencyId     = 5,
                ValidFrom      = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ValidTo        = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
                IsActive       = Status.Active
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesItemPriceMaster.FirstOrDefaultAsync(x => x.Id == id);

            saved!.ModifiedBy.Should().Be(1);
            saved.ModifiedByName.Should().Be("test-user");
            saved.ModifiedIP.Should().Be("127.0.0.1");
            saved.ModifiedDate.Should().NotBeNull();
        }

        // ── SoftDeleteAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(segmentId));
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(segmentId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesItemPriceMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = CreateRepository(ctx);

            var result = await repo.SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            var segmentId = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity(segmentId));
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
