using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public MiscMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscMasterQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedTypeAsync(string code = "MMQ_TYP")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var existing = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == code);
            if (existing != null) return existing.Id;
            var t = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code, Description = code,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscTypeMaster.AddAsync(t);
            await ctx.SaveChangesAsync();
            return t.Id;
        }

        private async Task<int> SeedAsync(int typeId, string code, string desc = "desc",
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = new Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId, Code = code, Description = desc, SortOrder = 1,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.MiscMaster.AddAsync(m);
            await ctx.SaveChangesAsync();
            return m.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            var t = await SeedTypeAsync();
            await SeedAsync(t, "MMQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_MiscTypeId()
        {
            await ClearAsync();
            var t1 = await SeedTypeAsync("MMQT1");
            var t2 = await SeedTypeAsync("MMQT2");
            await SeedAsync(t1, "M1");
            await SeedAsync(t2, "M2");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null, t1);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
            rows[0].MiscTypeId.Should().Be(t1);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var t = await SeedTypeAsync();
            var id = await SeedAsync(t, "MQID");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Code.Should().Be("MQID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var t = await SeedTypeAsync();
            var id = await SeedAsync(t, "MQSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            var t = await SeedTypeAsync();
            await SeedAsync(t, "MAC1", "ActiveDesc");
            await SeedAsync(t, "MAC2", "InactiveDesc", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("Active", null, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].Code.Should().Be("MAC1");
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate_Within_Type()
        {
            await ClearAsync();
            var t = await SeedTypeAsync();
            await SeedAsync(t, "MDUP1");

            var result = await CreateRepo().AlreadyExistsAsync("MDUP1", t);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var t = await SeedTypeAsync();
            var id = await SeedAsync(t, "MSELF");

            var result = await CreateRepo().AlreadyExistsAsync("MSELF", t, id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_True_For_Active_Type()
        {
            var t = await SeedTypeAsync();

            var result = await CreateRepo().MiscTypeExistsAsync(t);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().MiscTypeExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetMiscMasterByCode_Should_Return_Matching_When_Exists()
        {
            await ClearAsync();
            var t = await SeedTypeAsync();
            await SeedAsync(t, "GETBYC");

            var result = await CreateRepo().GetMiscMasterByCode("getbyc");

            result.Should().NotBeNull();
            result!.Code.Should().Be("GETBYC");
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Children()
        {
            await ClearAsync();
            var t = await SeedTypeAsync();
            var id = await SeedAsync(t, "MSDV1");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsMiscMasterLinkedAsync_Should_Return_False_When_No_Active_Children()
        {
            await ClearAsync();
            var t = await SeedTypeAsync();
            var id = await SeedAsync(t, "MLK1");

            var result = await CreateRepo().IsMiscMasterLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
