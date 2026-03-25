using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.MiscMaster;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;
using PurchaseManagement.IntegrationTests.Common;

namespace PurchaseManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterCommandRepository CreateRepository(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private MiscTypeMasterCommandRepository CreateMiscTypeRepo(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static PurchaseManagement.Domain.Entities.MiscMaster BuildEntity(
            int miscTypeId = 1,
            string code = "MC001",
            string description = "Test Misc Master") =>
            new()
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                IsActive = PurchaseManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };

        private async Task<int> SeedMiscTypeMasterAsync(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var entity = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "TYP001",
                Description = "Test Type",
                IsActive = PurchaseManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
            var result = await CreateMiscTypeRepo(ctx).CreateAsync(entity);
            return result.Id;
        }

        private async Task ClearTablesAsync(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PortMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscTypeMaster");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync(ctx);

            var entity = BuildEntity(miscTypeId);
            var result = await CreateRepository(ctx).CreateAsync(entity);

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync(ctx);

            var entity = BuildEntity(miscTypeId, "MC002", "My Description");
            var result = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);
            saved!.Code.Should().Be("MC002");
            saved.Description.Should().Be("My Description");
            saved.MiscTypeId.Should().Be(miscTypeId);
            saved.IsActive.Should().Be(PurchaseManagement.Domain.Common.BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Auto_Generate_SortOrder()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync(ctx);

            var entity = BuildEntity(miscTypeId);
            var result = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);
            saved!.SortOrder.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync(ctx);

            var entity = BuildEntity(miscTypeId);
            var result = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var toUpdate = BuildEntity(miscTypeId, "MC001", "Updated Description");
            toUpdate.Id = created.Id;
            var result = await CreateRepository(ctx).UpdateAsync(created.Id, toUpdate);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var toUpdate = BuildEntity(miscTypeId, "MC001", "Updated Description");
            toUpdate.Id = created.Id;
            await CreateRepository(ctx).UpdateAsync(created.Id, toUpdate);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == created.Id);
            saved!.Description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync(ctx);

            var toUpdate = BuildEntity(miscTypeId);
            var result = await CreateRepository(ctx).UpdateAsync(9999, toUpdate);

            result.Should().BeFalse();
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity(miscTypeId);
            entity.IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted;
            var result = await CreateRepository(ctx).DeleteAsync(created.Id, entity);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync(ctx);
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity(miscTypeId);
            entity.IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted;
            await CreateRepository(ctx).DeleteAsync(created.Id, entity);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);
            deleted!.IsDeleted.Should().Be(PurchaseManagement.Domain.Common.BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var entity = BuildEntity();
            var result = await CreateRepository(ctx).DeleteAsync(9999, entity);

            result.Should().BeFalse();
        }
    }
}
