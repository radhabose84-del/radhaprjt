using Microsoft.EntityFrameworkCore;
using QCManagement.Infrastructure.Data;
using QCManagement.Infrastructure.Repositories.MiscMaster;
using QCManagement.Infrastructure.Repositories.MiscTypeMaster;
using QCManagement.IntegrationTests.Common;
using static QCManagement.Domain.Common.BaseEntity;

namespace QCManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterCommandRepository CreateRepo(ApplicationDbContext ctx)
            => new MiscMasterCommandRepository(ctx);

        private async Task<int> SeedParentMiscTypeAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MiscTypeMasterCommandRepository(ctx)
                .CreateAsync(new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "QP_GROUP",
                    Description = "Quality Parameter Group",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
        }

        private Domain.Entities.MiscMaster BuildEntity(
            int miscTypeId,
            string code = "PHY",
            string description = "Physical",
            int sortOrder = 1) =>
            new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = description,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync() =>
            await _fixture.ClearTablesAsync("QC.QualitySpecificationParameter", "QC.QualitySpecification", "QC.QualityTemplateParameter", "QC.QualityTemplate", "QC.QualityParameter", "QC.MiscMaster", "QC.MiscTypeMaster");

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(parentId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(parentId, "PHY", "Physical", 1));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.MiscTypeId.Should().Be(parentId);
            saved.Code.Should().Be("PHY");
            saved.Description.Should().Be("Physical");
            saved.SortOrder.Should().Be(1);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity(parentId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == newId);
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(parentId, "PHY", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).UpdateAsync(new Domain.Entities.MiscMaster
            {
                Id = id,
                Description = "Updated Description",
                SortOrder = 99,
                IsActive = Status.Inactive
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.Description.Should().Be("Updated Description");
            saved.SortOrder.Should().Be(99);
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_Code()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(parentId, "ORIG", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).UpdateAsync(new Domain.Entities.MiscMaster
            {
                Id = id,
                Description = "Updated",
                SortOrder = 1,
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.Code.Should().Be("ORIG");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).UpdateAsync(new Domain.Entities.MiscMaster
            {
                Id = 99999,
                Description = "Ghost",
                SortOrder = 1,
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(parentId));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(parentId));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.MiscMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenEntityNotFound()
        {
            await ClearTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Highest_SortOrder()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateRepo(ctx).CreateAsync(BuildEntity(parentId, "PHY", "Physical", 1));
            await CreateRepo(ctx).CreateAsync(BuildEntity(parentId, "CHM", "Chemical", 5));
            await CreateRepo(ctx).CreateAsync(BuildEntity(parentId, "VIS", "Visual", 3));

            var max = await CreateRepo(ctx).GetMaxSortOrderAsync(parentId);

            max.Should().Be(5);
        }

        [Fact]
        public async Task GetMaxSortOrderAsync_Should_Return_Zero_WhenNoRecords()
        {
            await ClearTablesAsync();
            var parentId = await SeedParentMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var max = await CreateRepo(ctx).GetMaxSortOrderAsync(parentId);

            max.Should().Be(0);
        }
    }
}
