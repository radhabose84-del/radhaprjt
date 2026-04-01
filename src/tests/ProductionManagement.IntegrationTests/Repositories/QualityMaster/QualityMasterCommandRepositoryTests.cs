using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.QualityMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.QualityMaster
{
    [Collection("DatabaseCollection")]
    public sealed class QualityMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public QualityMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private QualityMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private static Domain.Entities.QualityMaster BuildEntity(
            string name = "Premium",
            string desc = "Premium quality") =>
            new()
            {
                QualityName = name,
                Description = desc,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx) =>
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [Production].[QualityMaster]");

        // --- CREATE ---

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity("Standard", "Standard quality"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualityMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.QualityName.Should().Be("Standard");
            saved.Description.Should().Be("Standard quality");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepo(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.QualityMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var entity = await ctx.QualityMaster.FirstAsync(x => x.Id == id);
            entity.QualityName = "Updated Quality";
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.QualityMaster.FirstAsync(x => x.Id == id);
            updated.QualityName.Should().Be("Updated Quality");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var entity = BuildEntity();
            entity.Id = 99999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.QualityMaster.IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
