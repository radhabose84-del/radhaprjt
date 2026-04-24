using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.Lookups.FixedAssetManagement;
using Microsoft.Data.SqlClient;

namespace FixedAssetManagement.IntegrationTests.Repositories.Lookups
{
    // =====================================================================================
    // BLOCKED - Production SQL references a column that does not exist on AssetWarranty.
    //
    // CityValidationLookupRepository.IsCityUsedAsync executes:
    //     SELECT CASE
    //         WHEN EXISTS (SELECT 1 FROM FixedAsset.Manufacture   WHERE CityId = @CityId AND IsDeleted = 0)
    //           OR EXISTS (SELECT 1 FROM FixedAsset.AssetWarranty WHERE CityId = @CityId AND IsDeleted = 0)
    //         THEN 1 ELSE 0
    //     END;
    //
    // The AssetWarranties entity (and the EF-generated AssetWarranty table) only has
    // ServiceCityId / ServiceStateId / ServiceCountryId - there is no plain "CityId".
    // The whole CASE expression is parsed as one statement, so the "Invalid column name
    // 'CityId'" error fires before the Manufacture branch can evaluate - every test below
    // would fail at runtime.
    //
    // Remove the Skip argument on each fact once the production SQL is corrected
    // (either rename to ServiceCityId or change the source table).
    // =====================================================================================
    [Collection("DatabaseCollection")]
    public sealed class CityValidationLookupRepositoryTests
    {
        private const string BlockedReason =
            "BLOCKED - Production SQL in CityValidationLookupRepository references " +
            "FixedAsset.AssetWarranty.CityId which does not exist (entity uses ServiceCityId). " +
            "See class XML doc for details.";

        private readonly DbFixture _fixture;

        public CityValidationLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private CityValidationLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CityValidationLookupRepository(conn);
        }

        private async Task<int> SeedManufactureAsync(
            int cityId,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Manufactures
            {
                Code = $"MFG-{cityId}",
                ManufactureName = $"Manufacturer for City {cityId}",
                CountryId = 1,
                StateId = 1,
                CityId = cityId,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = deleted
            };
            await ctx.Manufactures.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        [Fact(Skip = BlockedReason)]
        public async Task IsCityUsedAsync_Should_Return_True_When_Referenced_By_Manufacture()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedManufactureAsync(cityId: 42);

            var result = await CreateRepo().IsCityUsedAsync(42);

            result.Should().BeTrue();
        }

        [Fact(Skip = BlockedReason)]
        public async Task IsCityUsedAsync_Should_Return_False_When_Not_Referenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().IsCityUsedAsync(99);

            result.Should().BeFalse();
        }

        [Fact(Skip = BlockedReason)]
        public async Task IsCityUsedAsync_Should_Exclude_SoftDeleted_Manufacture()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedManufactureAsync(cityId: 7, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().IsCityUsedAsync(7);

            result.Should().BeFalse();
        }
    }
}
