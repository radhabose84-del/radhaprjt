using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Domain.Common;
using PurchaseEntities = PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Entities.PriceMaster;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.PriceMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PriceMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PriceMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IMiscMasterQueryRepository> _miscQueryRepo = new(MockBehavior.Loose);

        // Seeded MiscMaster IDs — set after SeedPrerequisitesAsync
        private int _sourceFromMiscId;
        private int _statusMiscId;

        public PriceMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PriceMasterCommandRepository CreateRepository(ApplicationDbContext ctx) =>
            new(ctx, _fixture.IpMock.Object, _miscQueryRepo.Object);

        private PriceMasterHeader BuildHeader(
            int itemId = 1,
            int vendorId = 1) =>
            new()
            {
                ItemId = itemId,
                VendorId = vendorId,
                UnitId = 1,
                UomId = 1,
                ValidFrom = DateOnly.FromDateTime(DateTime.Today),
                StatusId = _statusMiscId,
                SourceFromId = _sourceFromMiscId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                Details = new List<PriceMasterDetail>
                {
                    new()
                    {
                        ScaleQtyFrom = 1,
                        UnitPrice = 100m,
                        CurrencyId = 1,
                        IsActive = Status.Active,
                        IsDeleted = IsDelete.NotDeleted
                    }
                }
            };

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PriceMasterDetail");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PriceMasterHeader");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PortMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.DutyMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PaymentTermInstallment");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PaymentTermMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscTypeMaster");
        }

        /// <summary>
        /// Seeds MiscTypeMaster + MiscMaster rows required by PriceMasterHeader FK constraints
        /// (SourceFromId and StatusId both reference Purchase.MiscMaster).
        /// </summary>
        private async Task SeedPrerequisitesAsync(ApplicationDbContext ctx)
        {
            // MiscTypeMaster parent rows
            var miscType = new PurchaseEntities.MiscTypeMaster
            {
                MiscTypeCode = "PRICEMISC",
                Description = "Price Master Misc Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.Set<PurchaseEntities.MiscTypeMaster>().Add(miscType);
            await ctx.SaveChangesAsync();

            // MiscMaster for SourceFrom
            var sourceFrom = new PurchaseEntities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "SRCFROM",
                Description = "Source From",
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            // MiscMaster for Status
            var status = new PurchaseEntities.MiscMaster
            {
                MiscTypeId = miscType.Id,
                Code = "STATUS",
                Description = "Status",
                SortOrder = 2,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            ctx.Set<PurchaseEntities.MiscMaster>().AddRange(sourceFrom, status);
            await ctx.SaveChangesAsync();

            _sourceFromMiscId = sourceFrom.Id;
            _statusMiscId = status.Id;
        }

        // --- ADD + SAVE ---

        [Fact]
        public async Task AddAsync_And_SaveChanges_Should_Persist_Header()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedPrerequisitesAsync(ctx);
            var repo = CreateRepository(ctx);

            var header = BuildHeader(itemId: 10, vendorId: 20);
            await repo.AddAsync(header, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PriceMasterHeader.FirstOrDefaultAsync(x =>
                x.ItemId == 10 && x.VendorId == 20);

            saved.Should().NotBeNull();
            saved!.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task AddAsync_Should_Persist_With_Detail()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedPrerequisitesAsync(ctx);
            var repo = CreateRepository(ctx);

            var header = BuildHeader(itemId: 11, vendorId: 21);
            await repo.AddAsync(header, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PriceMasterHeader
                .Include(h => h.Details)
                .FirstOrDefaultAsync(x => x.ItemId == 11 && x.VendorId == 21);

            saved!.Details.Should().HaveCount(1);
            saved.Details.First().UnitPrice.Should().Be(100m);
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedPrerequisitesAsync(ctx);
            var repo = CreateRepository(ctx);

            var header = BuildHeader(itemId: 30, vendorId: 30);
            await repo.AddAsync(header, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(header.Id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_On_Header()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedPrerequisitesAsync(ctx);
            var repo = CreateRepository(ctx);

            var header = BuildHeader(itemId: 31, vendorId: 31);
            await repo.AddAsync(header, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(header.Id);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.PriceMasterHeader
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == header.Id);

            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999);

            result.Should().BeFalse();
        }

        // --- HAS OVERLAPPING HEADER ---

        [Fact]
        public async Task HasOverlappingHeaderAsync_Should_Return_True_When_Overlap_Exists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            await SeedPrerequisitesAsync(ctx);
            var repo = CreateRepository(ctx);

            var from = DateOnly.FromDateTime(DateTime.Today);
            var to = from.AddDays(30);
            var header = BuildHeader(itemId: 50, vendorId: 50);
            header.ValidFrom = from;
            header.ValidTo = to;
            await repo.AddAsync(header, CancellationToken.None);
            await repo.SaveChangesAsync(CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var overlaps = await CreateRepository(ctx).HasOverlappingHeaderAsync(
                50, 50, from.AddDays(5), to.AddDays(-5), CancellationToken.None);

            overlaps.Should().BeTrue();
        }

        [Fact]
        public async Task HasOverlappingHeaderAsync_Should_Return_False_When_No_Overlap()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var overlaps = await CreateRepository(ctx).HasOverlappingHeaderAsync(
                999, 999,
                DateOnly.FromDateTime(DateTime.Today),
                null,
                CancellationToken.None);

            overlaps.Should().BeFalse();
        }
    }
}
