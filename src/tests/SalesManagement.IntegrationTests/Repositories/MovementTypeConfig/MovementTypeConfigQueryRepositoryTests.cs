using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.MovementTypeConfig;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.MovementTypeConfig
{
    [Collection("DatabaseCollection")]
    public sealed class MovementTypeConfigQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public MovementTypeConfigQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MovementTypeConfigQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> EnsureMiscIdAsync(string code = "MTCQ_M")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "MTCQ_T");
            if (t == null)
            {
                t = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "MTCQ_T", Description = "T",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<int> SeedAsync(
            string code, Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var miscId = await EnsureMiscIdAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new SalesManagement.Domain.Entities.MovementTypeConfig
            {
                MovementCode = code,
                MovementDescription = $"D {code}",
                MovementCategoryId = miscId,
                FromStockTypeId = miscId,
                ToStockTypeId = miscId,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.MovementTypeConfig.AddAsync(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("MQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("MQDL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("UNQ");
            await SeedAsync("OTH");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNQ");

            rows.Should().HaveCount(1);
            rows[0].MovementCode.Should().Be("UNQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("MID");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.MovementCode.Should().Be("MID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("MSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("MAC1");
            await SeedAsync("MAC2", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("MAC", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("DUP");

            var result = await CreateRepo().AlreadyExistsAsync("DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("SELF");

            var result = await CreateRepo().AlreadyExistsAsync("SELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_True_For_Active()
        {
            var miscId = await EnsureMiscIdAsync();

            var result = await CreateRepo().MiscMasterExistsAsync(miscId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MiscMasterExistsAsync_Should_Return_False_For_Unknown()
        {
            var result = await CreateRepo().MiscMasterExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("SDV");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsMovementTypeConfigLinkedAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("MLK");

            var result = await CreateRepo().IsMovementTypeConfigLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
