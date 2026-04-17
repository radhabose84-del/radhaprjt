using Microsoft.EntityFrameworkCore;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.MiscMaster;
using PartyManagement.Infrastructure.Repositories.MiscTypeMaster;
using PartyManagement.Infrastructure.Repositories.PartyGroup;

namespace PartyManagement.IntegrationTests.Repositories.PartyGroup
{
    [Collection("DatabaseCollection")]
    public sealed class PartyGroupCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public PartyGroupCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private PartyGroupCommandRepository CreateRepository(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        /// <summary>Seeds MiscTypeMaster + two MiscMaster entries; returns (groupTypeId, glCategoryId).</summary>
        private async Task<(int groupTypeId, int glCategoryId)> SeedMiscPrerequisitesAsync()
        {
            // MiscTypeMaster
            await using var ctx1 = _fixture.CreateFreshDbContext();
            var mtRepo = new MiscTypeMasterCommandRepository(ctx1);
            var mt = await mtRepo.CreateAsync(new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "PG_TYPE",
                Description = "Party Group Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            // MiscMaster #1 — for GroupTypeId
            await using var ctx2 = _fixture.CreateFreshDbContext();
            var mmRepo = new MiscMasterCommandRepository(ctx2);
            var mm1 = await mmRepo.CreateAsync(new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = "PG_GRP_TYPE",
                Description = "Group Type",
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            // MiscMaster #2 — for GlCategoryId
            await using var ctx3 = _fixture.CreateFreshDbContext();
            var mmRepo2 = new MiscMasterCommandRepository(ctx3);
            var mm2 = await mmRepo2.CreateAsync(new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = mt.Id,
                Code = "PG_GL_CAT",
                Description = "GL Category",
                SortOrder = 2,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            return (mm1.Id, mm2.Id);
        }

        private static PartyManagement.Domain.Entities.PartyGroup BuildEntity(
            int groupTypeId,
            int glCategoryId,
            string name = "Test Party Group") =>
            new PartyManagement.Domain.Entities.PartyGroup
            {
                PartyGroupName = name,
                GroupTypeId = groupTypeId,
                GlCategoryId = glCategoryId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx) => await _fixture.ClearAllTablesAsync();

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupTypeId, glCategoryId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupTypeId, glCategoryId, "Alpha Group"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PartyGroup.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.PartyGroupName.Should().Be("Alpha Group");
            saved.GroupTypeId.Should().Be(groupTypeId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupTypeId, glCategoryId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.PartyGroup.FirstOrDefaultAsync(x => x.Id == newId);

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

            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupTypeId, glCategoryId, "Original Group"));
            ctx.ChangeTracker.Clear();

            var toUpdate = new PartyManagement.Domain.Entities.PartyGroup
            {
                Id = newId,
                PartyGroupName = "Updated Group",
                GroupTypeId = groupTypeId,
                GlCategoryId = glCategoryId,
                IsActive = BaseEntity.Status.Active
            };

            var result = await CreateRepository(ctx).UpdateAsync(newId, toUpdate);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupTypeId, glCategoryId, "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(newId, new PartyManagement.Domain.Entities.PartyGroup
            {
                Id = newId,
                PartyGroupName = "Updated",
                GroupTypeId = groupTypeId,
                GlCategoryId = glCategoryId,
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            var updated = await ctx.PartyGroup.FirstOrDefaultAsync(x => x.Id == newId);
            updated!.PartyGroupName.Should().Be("Updated");
        }

        // --- DELETE ---

        [Fact]
        public async Task DeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupTypeId, glCategoryId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAsync(newId,
                new PartyManagement.Domain.Entities.PartyGroup { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var (groupTypeId, glCategoryId) = await SeedMiscPrerequisitesAsync();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(groupTypeId, glCategoryId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(newId,
                new PartyManagement.Domain.Entities.PartyGroup { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.PartyGroup
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == newId);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }
    }
}
