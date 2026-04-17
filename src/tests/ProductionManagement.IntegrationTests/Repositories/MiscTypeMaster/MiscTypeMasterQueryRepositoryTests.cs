using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.MiscTypeMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public MiscTypeMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private MiscTypeMasterQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(string code, string desc = "desc",
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code, Description = desc, IsActive = active, IsDeleted = deleted
            };
            await ctx.MiscTypeMaster.AddAsync(t);
            await ctx.SaveChangesAsync();
            return t.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int typeId, string code = "MM1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var m = new Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId, Code = code, Description = code, SortOrder = 1,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
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
            await SeedAsync("MTQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("MTQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("MTQID");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.MiscTypeCode.Should().Be("MTQID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("MTQSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("MTAC1");
            await SeedAsync("MTAC2", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("MTAC", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].MiscTypeCode.Should().Be("MTAC1");
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("MTDUP1");

            var result = await CreateRepo().AlreadyExistsAsync("MTDUP1");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedAsync("MTSELF");

            var result = await CreateRepo().AlreadyExistsAsync("MTSELF", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_For_Existing()
        {
            await ClearAsync();
            var id = await SeedAsync("MTNF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Children()
        {
            await ClearAsync();
            var id = await SeedAsync("MTSDV1");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_Children_Exist()
        {
            await ClearAsync();
            var id = await SeedAsync("MTSDV2");
            await SeedMiscMasterAsync(id);

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsMiscTypeMasterLinkedAsync_Should_Return_True_When_Active_Children_Exist()
        {
            await ClearAsync();
            var id = await SeedAsync("MTLK");
            await SeedMiscMasterAsync(id);

            var result = await CreateRepo().IsMiscTypeMasterLinkedAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsMiscTypeMasterLinkedAsync_Should_Return_False_When_No_Active_Children()
        {
            await ClearAsync();
            var id = await SeedAsync("MTLK_NONE");

            var result = await CreateRepo().IsMiscTypeMasterLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
