using Dapper;
using Microsoft.Data.SqlClient;
using LogisticsManagement.Domain.Common;
using LogisticsManagement.Infrastructure.Data;
using LogisticsManagement.Infrastructure.Repositories.MiscTypeMaster;
using static LogisticsManagement.Domain.Common.BaseEntity;

namespace LogisticsManagement.IntegrationTests.Repositories.MiscTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class MiscTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public MiscTypeMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private MiscTypeMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new MiscTypeMasterQueryRepository(conn);
        }

        private async Task<int> SeedEntityAsync(string code = "MTCODE01", string description = "Test MiscType")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            // Clear FreightMaster first (FK to MiscMaster), then MiscMaster (FK to MiscTypeMaster), then MiscTypeMaster
            await conn.ExecuteAsync("DELETE FROM Logistics.FreightMaster");
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscMaster");
            await conn.ExecuteAsync("DELETE FROM Logistics.MiscTypeMaster");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Correct_Fields()
        {
            await ClearTableAsync();
            await SeedEntityAsync("TYPEABC", "Alpha Type");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].MiscTypeCode.Should().Be("TYPEABC");
            items[0].Description.Should().Be("Alpha Type");
            items[0].IsActive.Should().BeTrue();
            items[0].IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("TYPE001", "Alpha Type");
            await SeedEntityAsync("TYPE002", "Beta Type");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].Description.Should().Be("Alpha Type");
        }

        [Fact]
        public async Task GetAllAsync_Should_Support_Pagination()
        {
            await ClearTableAsync();
            for (int i = 1; i <= 5; i++)
                await SeedEntityAsync($"TYPE{i:D3}", $"Type {i}");

            var (page1, total) = await CreateQueryRepo().GetAllAsync(1, 2, null);
            var (page2, _) = await CreateQueryRepo().GetAllAsync(2, 2, null);

            total.Should().Be(5);
            page1.Should().HaveCount(2);
            page2.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_EmptyResult_ReturnsZeroCount()
        {
            await ClearTableAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("TYPEABC", "Test Name");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.MiscTypeCode.Should().Be("TYPEABC");
            dto.Description.Should().Be("Test Name");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(9999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("DUPECD");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DUPECD");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_Not_Exists()
        {
            await ClearTableAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync("NOCODE");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("DUPECD");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DUPECD");

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("DUPECD");

            var exists = await CreateQueryRepo().AlreadyExistsAsync("DUPECD", id);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            await ClearTableAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeTrue();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("TYPE001", "Alpha Type");
            await SeedEntityAsync("TYPE002", "Beta Type");

            var results = await CreateQueryRepo().AutocompleteAsync("Alpha", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].Description.Should().Be("Alpha Type");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("TYPE001", "Active Type");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.MiscTypeMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Active", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("TYPE001", "Deleted Type");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new MiscTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_EmptyTerm_Should_Return_All_Active()
        {
            await ClearTableAsync();
            await SeedEntityAsync("TYPE001", "Type One");
            await SeedEntityAsync("TYPE002", "Type Two");

            var results = await CreateQueryRepo().AutocompleteAsync("", CancellationToken.None);

            results.Should().HaveCount(2);
        }

        // --- SOFT DELETE VALIDATION ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Dependents()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            var hasLinks = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            hasLinks.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_MiscMaster_Dependent_Exists()
        {
            await ClearTableAsync();
            var miscTypeId = await SeedEntityAsync("FMODE", "Freight Mode");

            // Seed a dependent MiscMaster record
            await using var ctx = _fixture.CreateFreshDbContext();
            await new LogisticsManagement.Infrastructure.Repositories.MiscMaster.MiscMasterCommandRepository(ctx)
                .CreateAsync(new Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = "MM001",
                    Description = "Dependent MiscMaster",
                    SortOrder = 1,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });

            var hasLinks = await CreateQueryRepo().SoftDeleteValidationAsync(miscTypeId);

            hasLinks.Should().BeTrue();
        }
    }
}
