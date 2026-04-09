using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.Infrastructure.Repositories.PaymentTermMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.PaymentTermMaster
{
    [Collection("DatabaseCollection")]
    public sealed class PaymentTermMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PaymentTermMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private PaymentTermMasterCommandRepository CreateRepository(ApplicationDbContext ctx) => new(ctx);

        private static PurchaseManagement.Domain.Entities.PaymentTermMaster BuildEntity(
            int baselineTypeId,
            string code = "PT001",
            string description = "Net 30") =>
            new()
            {
                Code = code,
                Description = description,
                BaselineTypeId = baselineTypeId,
                CreditDays = 30,
                AdvancePercent = 0m,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PaymentTermInstallment");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PaymentTermMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PortMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.DutyMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PriceMasterDetail");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PriceMasterHeader");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscTypeMaster");
        }

        private async Task<int> SeedBaselineTypeAsync(ApplicationDbContext ctx)
        {
            var typeRepo = new MiscTypeMasterCommandRepository(ctx);
            var mt = await typeRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "BTY001",
                Description = "Baseline Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });

            var miscRepo = new MiscMasterCommandRepository(ctx);
            var misc = await miscRepo.CreateAsync(new PurchaseManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = "NDAYS",
                Description = "Net Days",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
            return misc.Id;
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(
                BuildEntity(baselineTypeId), CancellationToken.None);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(
                BuildEntity(baselineTypeId, "PT002", "Net 60"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PaymentTermMasters.FirstOrDefaultAsync(x => x.Id == id);
            saved!.Code.Should().Be("PT002");
            saved.Description.Should().Be("Net 60");
            saved.CreditDays.Should().Be(30);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);

            var id = await CreateRepository(ctx).CreateAsync(
                BuildEntity(baselineTypeId), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PaymentTermMasters.FirstOrDefaultAsync(x => x.Id == id);
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(baselineTypeId), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var toUpdate = BuildEntity(baselineTypeId, "PT001", "Updated Description");
            toUpdate.Id = id;
            var result = await CreateRepository(ctx).UpdateAsync(toUpdate, null);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(baselineTypeId), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var toUpdate = BuildEntity(baselineTypeId, "PT001", "Updated Net 90");
            toUpdate.Id = id;
            await CreateRepository(ctx).UpdateAsync(toUpdate, null);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PaymentTermMasters.FirstOrDefaultAsync(x => x.Id == id);
            saved!.Description.Should().Be("Updated Net 90");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);

            var toUpdate = BuildEntity(baselineTypeId, "PT001", "Does Not Exist");
            toUpdate.Id = 9999;
            var result = await CreateRepository(ctx).UpdateAsync(toUpdate, null);

            result.Should().BeFalse();
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(baselineTypeId), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var baselineTypeId = await SeedBaselineTypeAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(baselineTypeId), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(id);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.PaymentTermMasters
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);
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
    }
}
