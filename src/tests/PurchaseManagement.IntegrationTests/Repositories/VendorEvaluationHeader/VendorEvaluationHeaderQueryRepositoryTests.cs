using Contracts.Interfaces.Lookups.Party;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.VendorEvaluationHeader;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.VendorEvaluationHeader
{
    [Collection("DatabaseCollection")]
    public sealed class VendorEvaluationHeaderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public VendorEvaluationHeaderQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // Supplier lookup mocked Loose → GetActiveSupplierByIdAsync returns null → no VendorName enrichment (safe).
        private VendorEvaluationHeaderQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString), new Mock<ISupplierLookup>(MockBehavior.Loose).Object);

        private async Task<int> SeedAsync(string evaluationCode, int vendorId = 1, int month = 6, int year = 2026,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorEvaluationHeader
            {
                EvaluationCode = evaluationCode,
                VendorId = vendorId,
                EvaluationMonth = month,
                EvaluationYear = year,
                EvaluationDate = new DateTimeOffset(2026, 6, 15, 0, 0, 0, TimeSpan.Zero),
                TotalWeightedScore = 85.5m,
                GradeId = null,
                Remarks = "eval",
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.VendorEvaluationHeaders.AddAsync(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        private async Task<int> SeedGradeAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var g = new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorRatingGrade
            {
                GradeCode = "VEH_GRD", GradeName = "Excellent",
                MinScore = 90m, MaxScore = 100m, ActionTypeId = null, SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.VendorRatingGrades.AddAsync(g);
            await ctx.SaveChangesAsync();
            return g.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("VEHQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("VEHQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("VEHQ_UNIQ");
            await SeedAsync("VEHQ_OTHER", month: 7);

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "VEHQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].EvaluationCode.Should().Be("VEHQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("VEHQ_GBI");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.EvaluationCode.Should().Be("VEHQ_GBI");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("VEHQ_GSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("VEHQ_CK", vendorId: 5, month: 3, year: 2026);

            var result = await CreateRepo().CompositeKeyExistsAsync(5, 3, 2026);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("VEHQ_CKS", vendorId: 6, month: 4, year: 2026);

            var result = await CreateRepo().CompositeKeyExistsAsync(6, 4, 2026, id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task GradeExistsAsync_Should_Return_True_For_Seeded()
        {
            await ClearAsync();
            var gradeId = await SeedGradeAsync();

            var result = await CreateRepo().GradeExistsAsync(gradeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task GradeExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().GradeExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task CriteriaExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().CriteriaExistsAsync(9999999);
            result.Should().BeFalse();
        }
    }
}
