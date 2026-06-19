using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.VendorRatingGrade;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.VendorRatingGrade
{
    [Collection("DatabaseCollection")]
    public sealed class VendorRatingGradeCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public VendorRatingGradeCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private VendorRatingGradeCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        // ActionTypeId is nullable → left null to avoid the MiscMaster FK in these tests.
        private static PurchaseManagement.Domain.Entities.VendorEvaluation.VendorRatingGrade BuildEntity(
            string gradeCode = "GRD001", string gradeName = "Excellent") =>
            new()
            {
                GradeCode = gradeCode,
                GradeName = gradeName,
                MinScore = 90m,
                MaxScore = 100m,
                ActionDescription = "Preferred vendor",
                ActionTypeId = null,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("GRD_C1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("GRD_C2", "Good"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.VendorRatingGrades.FirstAsync(x => x.Id == id);
            saved.GradeCode.Should().Be("GRD_C2");
            saved.GradeName.Should().Be("Good");
            saved.MinScore.Should().Be(90m);
            saved.MaxScore.Should().Be(100m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("GRD_U1"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("GRD_U1");
            entity.Id = id;
            entity.GradeName = "Updated";
            entity.MinScore = 80m;
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.VendorRatingGrades.FirstAsync(x => x.Id == id);
            reloaded.GradeName.Should().Be("Updated");
            reloaded.MinScore.Should().Be(80m);
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Modify_GradeCode()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("GRD_IM"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("GRD_IM");
            entity.Id = id;
            entity.GradeCode = "CHANGED";
            entity.GradeName = "x";
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.VendorRatingGrades.FirstAsync(x => x.Id == id);
            reloaded.GradeCode.Should().Be("GRD_IM");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity("GRD_GHOST");
            entity.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_And_Flag()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("GRD_D1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.VendorRatingGrades.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);
            result.Should().BeFalse();
        }
    }
}
