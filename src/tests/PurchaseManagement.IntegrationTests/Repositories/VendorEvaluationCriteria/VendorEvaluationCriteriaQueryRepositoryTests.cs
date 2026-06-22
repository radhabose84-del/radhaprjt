using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.VendorEvaluationCriteria;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.VendorEvaluationCriteria
{
    [Collection("DatabaseCollection")]
    public sealed class VendorEvaluationCriteriaQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public VendorEvaluationCriteriaQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private VendorEvaluationCriteriaQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "VECQ_MT", Description = "VEC Type",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscTypeMaster>().AddAsync(mt);
            await ctx.SaveChangesAsync();
            var m = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                Code = "VECQ_MSC", Description = "VEC Misc", MiscTypeId = mt.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscMaster>().AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task<int> SeedAsync(int miscId, string criteriaCode, string criteriaName = "Quality",
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationCriteria
            {
                CriteriaCode = criteriaCode,
                CriteriaName = criteriaName,
                Description = "desc",
                WeightagePercent = 25m,
                ScoringMethodId = miscId,
                MinimumScore = 0m,
                RatingImpactId = miscId,
                SortOrder = 1,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.VendorEvaluationCriteria.AddAsync(e);
            await ctx.SaveChangesAsync();

            if (active == Status.Inactive)
            {
                e.IsActive = Status.Inactive;
                await ctx.SaveChangesAsync();
            }
            return e.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "VECQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "VECQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "VECQ_UNIQ", "Alpha");
            await SeedAsync(miscId, "VECQ_OTHER", "Beta");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "VECQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].CriteriaCode.Should().Be("VECQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "VECQ_GBI");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.CriteriaCode.Should().Be("VECQ_GBI");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "VECQ_GSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "VECQ_AC1", "Active criteria");
            await SeedAsync(miscId, "VECQ_AC2", "Inactive criteria", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("VECQ_AC", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].CriteriaCode.Should().Be("VECQ_AC1");
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            await SeedAsync(miscId, "VECQ_DUP");

            var result = await CreateRepo().AlreadyExistsAsync("VECQ_DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();
            var id = await SeedAsync(miscId, "VECQ_SELF");

            var result = await CreateRepo().AlreadyExistsAsync("VECQ_SELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ScoringMethodExistsAsync_Should_Return_True_For_Seeded()
        {
            await ClearAsync();
            var miscId = await EnsureMiscAsync();

            var result = await CreateRepo().ScoringMethodExistsAsync(miscId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task RatingImpactExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().RatingImpactExistsAsync(9999999);
            result.Should().BeFalse();
        }
    }
}
