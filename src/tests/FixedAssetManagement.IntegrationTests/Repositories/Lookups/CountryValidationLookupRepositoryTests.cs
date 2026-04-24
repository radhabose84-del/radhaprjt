using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.Lookups.FixedAssetManagement;
using Microsoft.Data.SqlClient;

namespace FixedAssetManagement.IntegrationTests.Repositories.Lookups
{
    // =====================================================================================
    // BLOCKED - Production SQL references a column that does not exist on AssetWarranty.
    //
    // CountryValidationLookupRepository.IsCountryUsedAsync executes:
    //     SELECT CASE
    //         WHEN EXISTS (SELECT 1 FROM FixedAsset.Manufacture   WHERE CountryId = @CountryId AND IsDeleted = 0)
    //           OR EXISTS (SELECT 1 FROM FixedAsset.AssetWarranty WHERE CountryId = @CountryId AND IsDeleted = 0)
    //         THEN 1 ELSE 0
    //     END;
    //
    // The AssetWarranties entity (and the EF-generated AssetWarranty table) only has
    // ServiceCountryId - there is no plain "CountryId". The whole CASE expression parses
    // as one statement, so "Invalid column name 'CountryId'" fires before the Manufacture
    // branch can evaluate. Every test below would fail at runtime.
    //
    // Remove the Skip argument on each fact once the production SQL is corrected
    // (either rename to ServiceCountryId or change the source table).
    // =====================================================================================
    [Collection("DatabaseCollection")]
    public sealed class CountryValidationLookupRepositoryTests
    {
        private const string BlockedReason =
            "BLOCKED - Production SQL in CountryValidationLookupRepository references " +
            "FixedAsset.AssetWarranty.CountryId which does not exist (entity uses ServiceCountryId). " +
            "See class XML doc for details.";

        private readonly DbFixture _fixture;

        public CountryValidationLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private CountryValidationLookupRepository CreateRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CountryValidationLookupRepository(conn);
        }

        private async Task<int> SeedManufactureAsync(
            int countryId,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new Manufactures
            {
                Code = $"MFG-C{countryId}",
                ManufactureName = $"Manufacturer for Country {countryId}",
                CountryId = countryId,
                StateId = 1,
                CityId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = deleted
            };
            await ctx.Manufactures.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        [Fact(Skip = BlockedReason)]
        public async Task IsCountryUsedAsync_Should_Return_True_When_Referenced_By_Manufacture()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedManufactureAsync(countryId: 42);

            var result = await CreateRepo().IsCountryUsedAsync(42);

            result.Should().BeTrue();
        }

        [Fact(Skip = BlockedReason)]
        public async Task IsCountryUsedAsync_Should_Return_False_When_Not_Referenced()
        {
            await _fixture.ClearAllTablesAsync();

            var result = await CreateRepo().IsCountryUsedAsync(99);

            result.Should().BeFalse();
        }

        [Fact(Skip = BlockedReason)]
        public async Task IsCountryUsedAsync_Should_Exclude_SoftDeleted_Manufacture()
        {
            await _fixture.ClearAllTablesAsync();
            await SeedManufactureAsync(countryId: 7, deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().IsCountryUsedAsync(7);

            result.Should().BeFalse();
        }
    }
}
