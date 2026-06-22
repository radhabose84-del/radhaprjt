using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Data;
using PurchaseManagement.Infrastructure.Repositories.VendorEvaluationCriteria;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.VendorEvaluationCriteria
{
    [Collection("DatabaseCollection")]
    public sealed class VendorEvaluationCriteriaCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public VendorEvaluationCriteriaCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private VendorEvaluationCriteriaCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        // ScoringMethodId + RatingImpactId are required same-module FKs → seed a MiscMaster row.
        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx)
        {
            var mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "VEC_MT", Description = "VEC Type",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscTypeMaster>().AddAsync(mt);
            await ctx.SaveChangesAsync();
            var m = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                Code = "VEC_MSC", Description = "VEC Misc", MiscTypeId = mt.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscMaster>().AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private static PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationCriteria BuildEntity(
            int miscId, string criteriaCode = "VEC001", string criteriaName = "Quality") =>
            new()
            {
                CriteriaCode = criteriaCode,
                CriteriaName = criteriaName,
                Description = "Quality assessment criteria",
                WeightagePercent = 25m,
                ScoringMethodId = miscId,
                MinimumScore = 0m,
                RatingImpactId = miscId,
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
            var miscId = await EnsureMiscAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "VEC_C1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await EnsureMiscAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "VEC_C2", "Delivery"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.VendorEvaluationCriteria.FirstAsync(x => x.Id == id);
            saved.CriteriaCode.Should().Be("VEC_C2");
            saved.CriteriaName.Should().Be("Delivery");
            saved.WeightagePercent.Should().Be(25m);
            saved.ScoringMethodId.Should().Be(miscId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await EnsureMiscAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "VEC_U1"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity(miscId, "VEC_U1");
            entity.Id = id;
            entity.CriteriaName = "Updated";
            entity.WeightagePercent = 40m;
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.VendorEvaluationCriteria.FirstAsync(x => x.Id == id);
            reloaded.CriteriaName.Should().Be("Updated");
            reloaded.WeightagePercent.Should().Be(40m);
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Modify_CriteriaCode()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await EnsureMiscAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "VEC_IM"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity(miscId, "VEC_IM");
            entity.Id = id;
            entity.CriteriaCode = "CHANGED";
            entity.CriteriaName = "x";
            await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.VendorEvaluationCriteria.FirstAsync(x => x.Id == id);
            reloaded.CriteriaCode.Should().Be("VEC_IM");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity(1, "VEC_GHOST");
            entity.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(entity);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_And_Flag()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscId = await EnsureMiscAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(miscId, "VEC_D1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.VendorEvaluationCriteria.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
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
