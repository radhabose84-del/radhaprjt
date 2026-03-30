using Microsoft.EntityFrameworkCore;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.MiscMaster;
using PartyManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace PartyManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterCommandRepository CreateRepository(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        /// <summary>Seeds a MiscTypeMaster and returns its Id for use as MiscTypeId FK.</summary>
        private async Task<int> SeedMiscTypeMasterAsync(string code = "MISC_TYPE_TEST")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var entity = new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test Misc Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
            var result = await repo.CreateAsync(entity);
            return result.Id;
        }

        private static PartyManagement.Domain.Entities.MiscMaster BuildEntity(int miscTypeId, string code = "MM001", string description = "Test Misc") =>
            new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Party].[PartyGroup]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Party].[MiscMaster]");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Party].[MiscTypeMaster]");
        }

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var miscTypeId = await SeedMiscTypeMasterAsync("MT_CREATE1");
            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var miscTypeId = await SeedMiscTypeMasterAsync("MT_CREATE2");
            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "MM002", "Test Description"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.Code.Should().Be("MM002");
            saved.Description.Should().Be("Test Description");
            saved.MiscTypeId.Should().Be(miscTypeId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var miscTypeId = await SeedMiscTypeMasterAsync("MT_AUDIT");
            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
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
            await ClearTableAsync(ctx);

            var miscTypeId = await SeedMiscTypeMasterAsync("MT_UPDATE1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "UPD001", "Original"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new PartyManagement.Domain.Entities.MiscMaster
            {
                Id = created.Id,
                MiscTypeId = miscTypeId,
                Code = "UPD001",
                Description = "Updated Description",
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(created.Id, toUpdate);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var miscTypeId = await SeedMiscTypeMasterAsync("MT_UPDATE2");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId, "UPD002", "Original"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new PartyManagement.Domain.Entities.MiscMaster
            {
                Id = created.Id,
                MiscTypeId = miscTypeId,
                Code = "UPD002",
                Description = "Updated Description",
                SortOrder = 2,
                IsActive = BaseEntity.Status.Active
            };

            await CreateRepository(ctx).UpdateAsync(created.Id, toUpdate);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == created.Id);
            updated!.Description.Should().Be("Updated Description");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var toUpdate = new PartyManagement.Domain.Entities.MiscMaster
            {
                Id = 9999,
                MiscTypeId = 1,
                Code = "NOTEXIST",
                Description = "Not Found",
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(9999, toUpdate);

            result.Should().BeFalse();
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var miscTypeId = await SeedMiscTypeMasterAsync("MT_DEL1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            var toDelete = new PartyManagement.Domain.Entities.MiscMaster { IsDeleted = BaseEntity.IsDelete.Deleted };
            var result = await CreateRepository(ctx).DeleteAsync(created.Id, toDelete);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var miscTypeId = await SeedMiscTypeMasterAsync("MT_DEL2");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(miscTypeId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id, new PartyManagement.Domain.Entities.MiscMaster { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.MiscMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == created.Id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999, new PartyManagement.Domain.Entities.MiscMaster { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeFalse();
        }
    }
}
