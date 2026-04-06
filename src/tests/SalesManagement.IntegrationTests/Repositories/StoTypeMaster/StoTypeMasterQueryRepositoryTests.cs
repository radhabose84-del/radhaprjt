using Dapper;
using Microsoft.Data.SqlClient;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.MovementTypeConfig;
using SalesManagement.Infrastructure.Repositories.StoTypeMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.StoTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class StoTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public StoTypeMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private StoTypeMasterQueryRepository CreateQueryRepo()
            => new StoTypeMasterQueryRepository(new SqlConnection(_fixture.ConnectionString));

        private StoTypeMasterCommandRepository CreateCommandRepo(ApplicationDbContext ctx)
            => new StoTypeMasterCommandRepository(ctx);

        private MovementTypeConfigCommandRepository CreateMtcRepo(ApplicationDbContext ctx)
            => new MovementTypeConfigCommandRepository(ctx);

        private Domain.Entities.StoTypeMaster BuildEntity(
            string code = "STO001",
            string name = "Test STO Type",
            int pgiId = 0,
            int grId = 0,
            bool isActive = true)
            => new Domain.Entities.StoTypeMaster
            {
                StoTypeCode = code,
                StoTypeName = name,
                Description = "Test Description",
                PgiMovementTypeId = pgiId,
                GrMovementTypeId = grId,
                IsActive = isActive ? Status.Active : Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM Sales.StoTypeMaster");
        }

        private async Task<(int pgiId, int grId)> EnsureMovementTypesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();

            var pgiId = await conn.ExecuteScalarAsync<int>(
                "SELECT TOP 1 Id FROM Sales.MovementTypeConfig WHERE IsDeleted = 0 AND IsActive = 1 ORDER BY Id");
            if (pgiId == 0)
            {
                await using var ctx = _fixture.CreateFreshDbContext();
                var repo = new MovementTypeConfigCommandRepository(ctx);
                pgiId = await repo.CreateAsync(new Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "QPGI001",
                    MovementDescription = "Query PGI MTC",
                    MovementCategoryId = 1,
                    FromStockTypeId = 2,
                    ToStockTypeId = 3,
                    QuantityUpdateFlag = true,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            }

            var grId = await conn.ExecuteScalarAsync<int>(
                "SELECT TOP 1 Id FROM Sales.MovementTypeConfig WHERE IsDeleted = 0 AND IsActive = 1 AND Id != @PgiId ORDER BY Id",
                new { PgiId = pgiId });
            if (grId == 0)
            {
                await using var ctx = _fixture.CreateFreshDbContext();
                var repo = new MovementTypeConfigCommandRepository(ctx);
                grId = await repo.CreateAsync(new Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "QGR001",
                    MovementDescription = "Query GR MTC",
                    MovementCategoryId = 1,
                    FromStockTypeId = 3,
                    ToStockTypeId = 2,
                    QuantityUpdateFlag = true,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
            }

            return (pgiId, grId);
        }

        private async Task<int> SeedAsync(Domain.Entities.StoTypeMaster entity)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            return await CreateCommandRepo(ctx).CreateAsync(entity);
        }

        // ── GetAllAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_Should_Return_SeededRecord()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            await SeedAsync(BuildEntity("STO001", "STO Type One", pgiId, grId));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            total.Should().Be(1);
            items.Should().HaveCount(1);
            items[0].StoTypeCode.Should().Be("STO001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            var id = await SeedAsync(BuildEntity("DELSTO001", "Deleted Type", pgiId, grId));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().NotContain(x => x.StoTypeCode == "DELSTO001");
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm_OnCode()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            await SeedAsync(BuildEntity("ALPHA001", "Alpha STO", pgiId, grId));
            await SeedAsync(BuildEntity("BETA001", "Beta STO", pgiId, grId));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "ALPHA");

            total.Should().Be(1);
            items[0].StoTypeCode.Should().Be("ALPHA001");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_BySearchTerm_OnName()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            await SeedAsync(BuildEntity("STO010", "Northern Type", pgiId, grId));
            await SeedAsync(BuildEntity("STO011", "Southern Type", pgiId, grId));

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Northern");

            total.Should().Be(1);
            items[0].StoTypeCode.Should().Be("STO010");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_WhenNoData()
        {
            await ClearTableAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // ── GetByIdAsync ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            var id = await SeedAsync(BuildEntity("BYID001", "ById STO Type", pgiId, grId));

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
            dto.StoTypeCode.Should().Be("BYID001");
            dto.StoTypeName.Should().Be("ById STO Type");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_WhenNotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(99999);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_AfterSoftDelete()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            var id = await SeedAsync(BuildEntity("SDEL001", "Soft Del STO", pgiId, grId));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // ── AlreadyExistsAsync ───────────────────────────────────────────────

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_WhenCodeExists()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            await SeedAsync(BuildEntity("EXISTS001", "Exists STO", pgiId, grId));

            var result = await CreateQueryRepo().AlreadyExistsAsync("EXISTS001");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenCodeDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().AlreadyExistsAsync("NOEXIST999");

            result.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_WhenExcludedId_Matches()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            var id = await SeedAsync(BuildEntity("EXCL001", "Excl STO", pgiId, grId));

            var result = await CreateQueryRepo().AlreadyExistsAsync("EXCL001", id);

            result.Should().BeFalse();
        }

        // ── NotFoundAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_WhenEntityExists()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            var id = await SeedAsync(BuildEntity(pgiId: pgiId, grId: grId));

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_WhenEntityDoesNotExist()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().NotFoundAsync(99999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_AfterSoftDelete()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            var id = await SeedAsync(BuildEntity(pgiId: pgiId, grId: grId));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().NotFoundAsync(id);

            result.Should().BeTrue();
        }

        // ── AutocompleteAsync ────────────────────────────────────────────────

        [Fact]
        public async Task AutocompleteAsync_Should_Return_MatchingResults()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            await SeedAsync(BuildEntity("ACM001", "Autocomplete STO Match", pgiId, grId));
            await SeedAsync(BuildEntity("XYZ001", "Other STO Type", pgiId, grId));

            var results = await CreateQueryRepo().AutocompleteAsync("Autocomplete", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].StoTypeCode.Should().Be("ACM001");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            await SeedAsync(BuildEntity("ACTV001", "Active STO", pgiId, grId, isActive: true));
            await SeedAsync(BuildEntity("INAC001", "Inactive STO", pgiId, grId, isActive: false));

            var results = await CreateQueryRepo().AutocompleteAsync("STO", CancellationToken.None);

            results.Should().NotContain(r => r.StoTypeCode == "INAC001");
            results.Should().Contain(r => r.StoTypeCode == "ACTV001");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var (pgiId, grId) = await EnsureMovementTypesAsync();
            var id = await SeedAsync(BuildEntity("DLAUTO001", "Deleted STO Auto", pgiId, grId));

            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var results = await CreateQueryRepo().AutocompleteAsync("Deleted", CancellationToken.None);

            results.Should().NotContain(r => r.StoTypeCode == "DLAUTO001");
        }
    }
}
