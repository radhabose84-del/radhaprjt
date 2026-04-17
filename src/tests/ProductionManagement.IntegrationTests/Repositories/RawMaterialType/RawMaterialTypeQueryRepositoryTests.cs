using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.RawMaterialType;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.RawMaterialType
{
    [Collection("DatabaseCollection")]
    public sealed class RawMaterialTypeQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public RawMaterialTypeQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private RawMaterialTypeQueryRepository CreateRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedAsync(
            string code, string? name = null,
            Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var e = new Domain.Entities.RawMaterialType
            {
                RawMaterialTypeCode = code,
                RawMaterialTypeName = name ?? code,
                Description = "desc",
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.RawMaterialType.AddAsync(e);
            await ctx.SaveChangesAsync();
            return e.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_With_TotalCount()
        {
            await ClearAsync();
            await SeedAsync("RMQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("RMQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("RMQ_UNIQ", "Unique");
            await SeedAsync("RMQ_OTH", "Other");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "RMQ_UNIQ");

            rows.Should().HaveCount(1);
            rows[0].RawMaterialTypeCode.Should().Be("RMQ_UNIQ");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("RMQID");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.RawMaterialTypeCode.Should().Be("RMQID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("RMQSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("RMQAC1", "Active1");
            await SeedAsync("RMQAC2", "Inactive1", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("Active", CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].RawMaterialTypeCode.Should().Be("RMQAC1");
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("RMDUP1");

            var result = await CreateRepo().AlreadyExistsAsync("RMDUP1");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_On_Update()
        {
            await ClearAsync();
            var id = await SeedAsync("RMSELF1");

            var result = await CreateRepo().AlreadyExistsAsync("RMSELF1", id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NameAlreadyExistsAsync_Should_Detect_Duplicate_Name()
        {
            await ClearAsync();
            await SeedAsync("RMN1", "DupName");

            var result = await CreateRepo().NameAlreadyExistsAsync("DupName");

            result.Should().BeTrue();
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
            var id = await SeedAsync("RMNF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task IsRawMaterialTypeLinkedAsync_Should_Always_Return_False()
        {
            // Reserved for Rule #25 — currently no dependent table, always returns false.
            var result = await CreateRepo().IsRawMaterialTypeLinkedAsync(123);
            result.Should().BeFalse();
        }
    }
}
