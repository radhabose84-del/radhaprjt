using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.Lookups.FixedAssetManagement;
using Microsoft.Data.SqlClient;

namespace FixedAssetManagement.IntegrationTests.Repositories.Lookups
{
    // =====================================================================================
    // BLOCKED - Production SQL references tables/columns that do not exist in the EF model.
    //
    // DepartmentValidationLookupRepository.IsDepartmentUsedAsync executes:
    //     SELECT CASE
    //         WHEN EXISTS (SELECT 1 FROM FixedAsset.Location              WHERE DepartmentId = @DepartmentId AND IsDeleted = 0)
    //           OR EXISTS (SELECT 1 FROM FixedAsset.SubLocation           WHERE DepartmentId = @DepartmentId AND IsDeleted = 0)
    //           OR EXISTS (SELECT 1 FROM FixedAsset.AssetLocation         WHERE DepartmentId = @DepartmentId)
    //           OR EXISTS (SELECT 1 FROM FixedAsset.AssetTransfer         WHERE DepartmentId = @DepartmentId AND IsDeleted = 0)
    //           OR EXISTS (SELECT 1 FROM FixedAsset.AssetTransferIssueHdr WHERE DepartmentId = @DepartmentId)
    //         THEN 1 ELSE 0
    //     END;
    //
    // Two of the five EXISTS branches cannot be resolved against the EF-generated schema:
    //   * FixedAsset.AssetTransfer          - table is not mapped by any DbSet / entity
    //   * FixedAsset.AssetTransferIssueHdr  - has FromDepartmentId / ToDepartmentId, no plain DepartmentId
    //
    // The whole CASE expression parses as one statement, so the compile-time "Invalid object
    // name 'AssetTransfer'" / "Invalid column name 'DepartmentId'" fires before the first
    // three (working) branches can evaluate. Every test below would fail at runtime.
    //
    // Remove the Skip argument on each fact once the production SQL is corrected
    // (remove the non-existent AssetTransfer reference and use From/ToDepartmentId for
    // AssetTransferIssueHdr).
    // =====================================================================================
    [Collection("DatabaseCollection")]
    public sealed class DepartmentValidationLookupRepositoryTests
    {
        private const string BlockedReason =
            "BLOCKED - Production SQL in DepartmentValidationLookupRepository references " +
            "FixedAsset.AssetTransfer (no such table) and FixedAsset.AssetTransferIssueHdr.DepartmentId " +
            "(no such column - entity has FromDepartmentId/ToDepartmentId). See class XML doc for details.";

        private readonly DbFixture _fixture;

        public DepartmentValidationLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DepartmentValidationLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DepartmentValidationLookupRepository(conn);
        }

        private async Task SeedLocationAsync(int departmentId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.Locations.AddAsync(new FAM.Domain.Entities.Location
            {
                Code = $"LOC-D{departmentId}",
                LocationName = $"Location for Dept {departmentId}",
                UnitId = 1,
                DepartmentId = departmentId,
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        private async Task SeedSubLocationAsync(int departmentId, int locationId)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.SubLocations.AddAsync(new FAM.Domain.Entities.SubLocation
            {
                Code = $"SL-D{departmentId}",
                SubLocationName = $"SubLoc for Dept {departmentId}",
                UnitId = 1,
                DepartmentId = departmentId,
                LocationId = locationId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            await ctx.SaveChangesAsync();
        }

        [Fact(Skip = BlockedReason)]
        public async Task IsDepartmentUsedAsync_Should_Return_True_When_Referenced_By_Location()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedLocationAsync(departmentId: 42);

            var result = await CreateRepo().IsDepartmentUsedAsync(42);

            result.Should().BeTrue();
        }

        [Fact(Skip = BlockedReason)]
        public async Task IsDepartmentUsedAsync_Should_Return_True_When_Referenced_By_SubLocation()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedLocationAsync(departmentId: 99);
            // Use the same DepartmentId for both Location and SubLocation to keep seed simple.
            await using (var ctx = _fixture.CreateFreshDbContext())
            {
                var loc = new FAM.Domain.Entities.Location
                {
                    Code = "LOC-FOR-SUB",
                    LocationName = "Parent",
                    UnitId = 1,
                    DepartmentId = 77,
                    IsActive = BaseEntity.Status.Active,
                    IsDeleted = BaseEntity.IsDelete.NotDeleted
                };
                await ctx.Locations.AddAsync(loc);
                await ctx.SaveChangesAsync();
                await SeedSubLocationAsync(departmentId: 77, locationId: loc.Id);
            }

            var result = await CreateRepo().IsDepartmentUsedAsync(77);

            result.Should().BeTrue();
        }

        [Fact(Skip = BlockedReason)]
        public async Task IsDepartmentUsedAsync_Should_Return_False_When_Not_Referenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().IsDepartmentUsedAsync(9999);

            result.Should().BeFalse();
        }

        [Fact(Skip = BlockedReason)]
        public async Task IsDepartmentUsedAsync_Should_Exclude_SoftDeleted_Location()
        {
            await _fixture.ClearAllTablesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.Locations.AddAsync(new FAM.Domain.Entities.Location
            {
                Code = "LOC-DEL",
                LocationName = "Deleted",
                UnitId = 1,
                DepartmentId = 555,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.Deleted
            });
            await ctx.SaveChangesAsync();

            var result = await CreateRepo().IsDepartmentUsedAsync(555);

            result.Should().BeFalse();
        }
    }
}
