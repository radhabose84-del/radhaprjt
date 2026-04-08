using Dapper;
using Microsoft.Data.SqlClient;
using LogisticsManagement.Domain.Common;
using LogisticsManagement.Infrastructure.Data;
using LogisticsManagement.Infrastructure.Repositories.MiscMaster;
using LogisticsManagement.Infrastructure.Repositories.MiscTypeMaster;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.IntegrationTests.Repositories.MiscMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscMasterQueryRepository(conn);
        }

        private async Task<int> SeedMiscTypeAsync(string code = "FMODE", string description = "Freight Mode")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MiscTypeMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = description,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
        }

        private async Task<int> SeedEntityAsync(
            int miscTypeId,
            string code = "MM001",
            string description = "Test MiscMaster",
            int sortOrder = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MiscMasterCommandRepository(ctx).CreateAsync(
                new Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = description,
                    SortOrder = sortOrder,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Logistics.FreightMaster");
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscMaster");
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscTypeMaster");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedEntityAsync(miscTypeId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_JoinedFields()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            await SeedEntityAsync(miscTypeId, "MM001", "Test Item");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].MiscTypeCode.Should().Be("FMODE");
            items[0].MiscTypeDescription.Should().Be("Freight Mode");
            items[0].Code.Should().Be("MM001");
            items[0].Description.Should().Be("Test Item");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedEntityAsync(miscTypeId, "MM001", "Alpha Item");
            await SeedEntityAsync(miscTypeId, "MM002", "Beta Item");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].Description.Should().Be("Alpha Item");
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            for (int i = 1; i <= 5; i++)
                await SeedEntityAsync(miscTypeId, $"MM{i:D3}", $"Item {i}", i);

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 2, null);

            total.Should().Be(5);
            page1.Should().HaveCount(2);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            var id = await SeedEntityAsync(miscTypeId, "MM001", "Test Name", 3);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Code.Should().Be("MM001");
            dto.Description.Should().Be("Test Name");
            dto.SortOrder.Should().Be(3);
            dto.MiscTypeCode.Should().Be("FMODE");
            dto.MiscTypeDescription.Should().Be("Freight Mode");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(9999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- ALREADY EXISTS (composite: code + miscTypeId) ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            await SeedEntityAsync(miscTypeId, "DUPECD");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DUPECD", miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NOCODE", miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId, "DUPECD");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DUPECD", miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId, "DUPECD");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DUPECD", miscTypeId, id);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Same_Code_Different_MiscType_Should_Return_False()
        {
            await ClearTablesAsync();
            var miscTypeId1 = await SeedMiscTypeAsync("TYPE01", "Type 1");
            var miscTypeId2 = await SeedMiscTypeAsync("TYPE02", "Type 2");
            await SeedEntityAsync(miscTypeId1, "SAMECODE");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("SAMECODE", miscTypeId2);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            await ClearTablesAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        // --- MISC TYPE EXISTS ---

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_True_When_Active()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            var exists = await CreateQueryRepo().MiscTypeExistsAsync(miscTypeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_False_When_Inactive()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.MiscTypeMaster.FirstAsync(x => x.Id == miscTypeId);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var exists = await CreateQueryRepo().MiscTypeExistsAsync(miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_False_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(miscTypeId, CancellationToken.None);

            var exists = await CreateQueryRepo().MiscTypeExistsAsync(miscTypeId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task MiscTypeExistsAsync_Should_Return_False_When_NotExists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().MiscTypeExistsAsync(9999);

            exists.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            await SeedEntityAsync(miscTypeId, "MM001", "Alpha Item");
            await SeedEntityAsync(miscTypeId, "MM002", "Beta Item");

            var results = await CreateQueryRepo().AutocompleteAsync("Alpha", null, CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].Description.Should().Be("Alpha Item");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Filter_By_MiscTypeCode()
        {
            await ClearTablesAsync();
            var miscTypeId1 = await SeedMiscTypeAsync("FMODE", "Freight Mode");
            var miscTypeId2 = await SeedMiscTypeAsync("RMETHOD", "Rate Method");
            await SeedEntityAsync(miscTypeId1, "MM001", "Mode Item");
            await SeedEntityAsync(miscTypeId2, "MM002", "Method Item");

            var results = await CreateQueryRepo().AutocompleteAsync("", "FMODE", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].Description.Should().Be("Mode Item");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId, "MM001", "Active Item");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.MiscMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Active", null, CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeAsync();
            var id = await SeedEntityAsync(miscTypeId, "MM001", "Deleted Item");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", null, CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
