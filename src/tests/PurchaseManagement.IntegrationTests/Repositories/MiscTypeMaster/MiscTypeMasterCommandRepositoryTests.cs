using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace PurchaseManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscTypeMasterCommandRepository CreateRepository(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private static PurchaseManagement.Domain.Entities.MiscTypeMaster BuildEntity(
            string code = "PURCH_TYPE",
            string description = "Purchase Type") =>
            new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(PurchaseManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.PortMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.DutyMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Purchase.MiscMaster");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Purchase].[MiscTypeMaster]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity("PURCH_TYPE", "Purchase Type"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved.MiscTypeCode.Should().Be("PURCH_TYPE");
            saved.Description.Should().Be("Purchase Type");
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("CODE001", "Original"));
            ctx.ChangeTracker.Clear();

            created.Description = "Updated Description";
            var result = await CreateRepository(ctx).UpdateAsync(created.Id, created);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity("CODE001", "Original"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                Id = created.Id,
                MiscTypeCode = "CODE001",
                Description = "Updated Description",
                IsActive = BaseEntity.Status.Active
            };

            await CreateRepository(ctx).UpdateAsync(created.Id, toUpdate);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.Id == created.Id);
            updated.Description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                Id = 9999,
                MiscTypeCode = "NOTEXIST",
                Description = "Not Found",
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(9999, entity);

            result.Should().BeFalse();
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var toDelete = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(created.Id, toDelete);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var created = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var toDelete = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            await CreateRepository(ctx).DeleteAsync(created.Id, toDelete);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscTypeMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var toDelete = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            };

            var result = await CreateRepository(ctx).DeleteAsync(9999, toDelete);

            result.Should().BeFalse();
        }
    }
}
