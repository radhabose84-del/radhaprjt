using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductionManagement.Infrastructure.Repositories.CountMaster;
using ProductionManagement.Infrastructure.Repositories.MiscMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.CountMaster
{
    [Collection("DatabaseCollection")]
    public sealed class CountMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CountMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private CountMasterQueryRepository CreateQueryRepo(Mock<IUOMLookup> uomLookup = null)
        {
            uomLookup ??= BuildDefaultUomLookup();
            return new CountMasterQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                uomLookup.Object);
        }

        private static Mock<IUOMLookup> BuildDefaultUomLookup(int uomId = 1, string code = "KG", string name = "Kilogram")
        {
            var mock = new Mock<IUOMLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto>
                {
                    new() { Id = uomId, Code = code, UOMName = name }
                });
            return mock;
        }

        private async Task<int> SeedMiscTypeAsync(string code = "MT01")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var miscType = new Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscTypeMaster.Add(miscType);
            await ctx.SaveChangesAsync();
            return miscType.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "MM01", string desc = "Test Misc")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new MiscMasterCommandRepository(ctx).CreateAsync(new Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = desc,
                SortOrder = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedCountMasterAsync(
            int countTypeId,
            string code = "C001",
            string desc = "Test Count",
            int? countCategoryId = null,
            int uomId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await new CountMasterCommandRepository(ctx).CreateAsync(new Domain.Entities.CountMaster
            {
                CountCode = code,
                CountValue = 10.5m,
                ShortName = "TST",
                CountCategoryId = countCategoryId,
                CountTypeId = countTypeId,
                CountDescription = desc,
                UOMId = uomId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Production].[CountMaster]");
            await conn.ExecuteAsync("DELETE FROM [Production].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Production].[MiscTypeMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId, "CTYPE", "Count Type");
            await SeedCountMasterAsync(countTypeId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CountTypeName()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId, "CTYPE", "Yarn Count Type");
            await SeedCountMasterAsync(countTypeId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].CountTypeName.Should().Be("Yarn Count Type");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_UOM_From_Lookup()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId);
            await SeedCountMasterAsync(countTypeId, uomId: 5);

            var uomMock = BuildDefaultUomLookup(5, "MTR", "Meter");
            var (items, _) = await CreateQueryRepo(uomMock).GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            items[0].UOMCode.Should().Be("MTR");
            items[0].UOMName.Should().Be("Meter");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId);
            var id = await SeedCountMasterAsync(countTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new CountMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId);
            await SeedCountMasterAsync(countTypeId, "C001", "Alpha Count");
            await SeedCountMasterAsync(countTypeId, "C002", "Beta Count");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].CountDescription.Should().Be("Alpha Count");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId, "CTYPE", "Type Desc");
            var id = await SeedCountMasterAsync(countTypeId, "C001", "ById Test");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.CountCode.Should().Be("C001");
            dto.CountDescription.Should().Be("ById Test");
            dto.CountTypeName.Should().Be("Type Desc");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId);
            var id = await SeedCountMasterAsync(countTypeId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new CountMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotExists()
        {
            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId);
            await SeedCountMasterAsync(countTypeId, "C001", "Alpha Count");
            await SeedCountMasterAsync(countTypeId, "C002", "Beta Count");

            var results = await CreateQueryRepo().AutocompleteAsync("Alpha", CancellationToken.None);

            results.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId);
            var id = await SeedCountMasterAsync(countTypeId, "C001", "Inactive Test");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.CountMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Inactive", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- GET NEXT COUNT CODE ---

        [Fact]
        public async Task GetNextCountCodeAsync_Should_Return_Next_Sequence()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId);
            await SeedCountMasterAsync(countTypeId, "1", "First");
            await SeedCountMasterAsync(countTypeId, "2", "Second");

            var next = await CreateQueryRepo().GetNextCountCodeAsync();

            next.Should().Be("3");
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            var notFound = await CreateQueryRepo().NotFoundAsync(99999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var countTypeId = await SeedMiscMasterAsync(mtId);
            var id = await SeedCountMasterAsync(countTypeId);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        // --- COUNT TYPE EXISTS ---

        [Fact]
        public async Task CountTypeExistsAsync_Should_Return_True_When_Active()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var miscId = await SeedMiscMasterAsync(mtId);

            var exists = await CreateQueryRepo().CountTypeExistsAsync(miscId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CountTypeExistsAsync_Should_Return_False_When_Missing()
        {
            var exists = await CreateQueryRepo().CountTypeExistsAsync(99999);

            exists.Should().BeFalse();
        }

        // --- COUNT CATEGORY EXISTS ---

        [Fact]
        public async Task CountCategoryExistsAsync_Should_Return_True_When_Active()
        {
            await ClearTablesAsync();
            var mtId = await SeedMiscTypeAsync();
            var miscId = await SeedMiscMasterAsync(mtId);

            var exists = await CreateQueryRepo().CountCategoryExistsAsync(miscId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CountCategoryExistsAsync_Should_Return_False_When_Missing()
        {
            var exists = await CreateQueryRepo().CountCategoryExistsAsync(99999);

            exists.Should().BeFalse();
        }
    }
}
