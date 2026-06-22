using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchaseManagement.Infrastructure.Repositories.VendorRatingGrade;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.VendorRatingGrade
{
    [Collection("DatabaseCollection")]
    public sealed class VendorRatingGradeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public VendorRatingGradeQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private VendorRatingGradeQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string gradeCode, string gradeName = "Excellent",
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new PurchaseManagement.Domain.Entities.VendorEvaluation.VendorRatingGrade
            {
                GradeCode = gradeCode,
                GradeName = gradeName,
                MinScore = 90m,
                MaxScore = 100m,
                ActionDescription = "Preferred vendor",
                ActionTypeId = null,
                SortOrder = 1,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.VendorRatingGrades.AddAsync(e);
            await ctx.SaveChangesAsync();

            if (active == Status.Inactive)
            {
                e.IsActive = Status.Inactive;
                await ctx.SaveChangesAsync();
            }
            return e.Id;
        }

        private async Task<int> SeedMiscMasterAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = new PurchaseManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "VRG_AT", Description = "Action Type",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscTypeMaster>().AddAsync(mt);
            await ctx.SaveChangesAsync();
            var m = new PurchaseManagement.Domain.Entities.MiscMaster
            {
                Code = "VRG_APPROVE", Description = "Approve", MiscTypeId = mt.Id,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.Set<PurchaseManagement.Domain.Entities.MiscMaster>().AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task ClearAsync() => await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("VRGQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("VRGQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("VRGQ_UNIQ", "Alpha grade");
            await SeedAsync("VRGQ_OTHER", "Beta grade");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "VRGQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].GradeCode.Should().Be("VRGQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("VRGQ_GBI");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.GradeCode.Should().Be("VRGQ_GBI");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("VRGQ_GSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedAsync("VRGQ_AC1", "Active grade");
            await SeedAsync("VRGQ_AC2", "Inactive grade", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("VRGQ_AC", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].GradeCode.Should().Be("VRGQ_AC1");
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("VRGQ_DUP");

            var result = await CreateRepo().AlreadyExistsAsync("VRGQ_DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("VRGQ_SELF");

            var result = await CreateRepo().AlreadyExistsAsync("VRGQ_SELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ActionTypeExistsAsync_Should_Return_True_For_Seeded()
        {
            await ClearAsync();
            var miscId = await SeedMiscMasterAsync();

            var result = await CreateRepo().ActionTypeExistsAsync(miscId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ActionTypeExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().ActionTypeExistsAsync(9999999);
            result.Should().BeFalse();
        }
    }
}
