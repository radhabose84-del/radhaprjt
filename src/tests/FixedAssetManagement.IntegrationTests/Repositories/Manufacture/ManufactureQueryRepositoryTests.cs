using Contracts.Interfaces.Validations.MaintenanceManagement;
using Dapper;
using Microsoft.Data.SqlClient;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.Manufacture;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.Manufacture
{
    [Collection("DatabaseCollection")]
    public sealed class ManufactureQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ManufactureQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ManufactureQueryRepository CreateQueryRepo(bool linked = false, bool active = false)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            var validation = new Mock<IMaintenanceManufacturerValidation>(MockBehavior.Loose);
            validation.Setup(v => v.HasLinkedManufacturerAsync(It.IsAny<int>())).ReturnsAsync(linked);
            validation.Setup(v => v.HasActiveManufacturerAsync(It.IsAny<int>())).ReturnsAsync(active);
            return new ManufactureQueryRepository(conn, validation.Object);
        }

        private async Task<int> SeedMfgTypeAsync(string typeCode)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var typeId = (await new MiscTypeMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = typeCode,
                Description = "Mfg Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;

            return (await new MiscMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId,
                Code = "MM_" + typeCode,
                Description = "Type Misc",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
        }

        private async Task<int> SeedEntityAsync(int mfgTypeId, string code = "MFGQ_001", string name = "Mfg Q")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var result = await new ManufactureCommandRepository(ctx).CreateAsync(new Manufactures
            {
                Code = code,
                ManufactureName = name,
                ManufactureType = mfgTypeId,
                CountryId = 1,
                StateId = 1,
                CityId = 1,
                AddressLine1 = "Addr",
                AddressLine2 = "Addr2",
                PinCode = "123456",
                PersonName = "Test Person",
                PhoneNumber = "1234567890",
                Email = "test@example.com",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllManufactureAsync_Should_Return_Seeded()
        {
            await ClearTablesAsync();
            var typeId = await SeedMfgTypeAsync("MFGQ_T1");
            await SeedEntityAsync(typeId);

            var (items, total) = await CreateQueryRepo().GetAllManufactureAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllManufactureAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var typeId = await SeedMfgTypeAsync("MFGQ_T2");
            await SeedEntityAsync(typeId, "MFGQ_A", "Alpha");
            await SeedEntityAsync(typeId, "MFGQ_B", "Beta");

            var (items, _) = await CreateQueryRepo().GetAllManufactureAsync(1, 10, "Alpha");

            items.Should().HaveCount(1);
            items[0].ManufactureName.Should().Be("Alpha");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTablesAsync();
            var typeId = await SeedMfgTypeAsync("MFGQ_T3");
            var id = await SeedEntityAsync(typeId, "MFGQ_ID", "ById");

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Code.Should().Be("MFGQ_ID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_KeyNotFoundException_When_NotFound()
        {
            await ClearTablesAsync();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => CreateQueryRepo().GetByIdAsync(9999));
        }

        [Fact]
        public async Task GetByManufactureNameAsync_Should_Return_Matching_Records()
        {
            await ClearTablesAsync();
            var typeId = await SeedMfgTypeAsync("MFGQ_T4");
            await SeedEntityAsync(typeId, "MFGQ_AC", "AutoMfg");

            var result = await CreateQueryRepo().GetByManufactureNameAsync("Auto");

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetManufactureTypeAsync_Should_Return_Empty_When_No_Type_Codes_Match()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetManufactureTypeAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task CountrySoftDeleteValidation_Should_Return_True_When_Linked()
        {
            await ClearTablesAsync();
            var typeId = await SeedMfgTypeAsync("MFGQ_T5");
            await SeedEntityAsync(typeId);

            (await CreateQueryRepo().CountrySoftDeleteValidation(1)).Should().BeTrue();
        }

        [Fact]
        public async Task CountrySoftDeleteValidation_Should_Return_False_When_NotLinked()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo().CountrySoftDeleteValidation(9999)).Should().BeFalse();
        }

        [Fact]
        public async Task StateSoftDeleteValidation_Should_Return_True_When_Linked()
        {
            await ClearTablesAsync();
            var typeId = await SeedMfgTypeAsync("MFGQ_T6");
            await SeedEntityAsync(typeId);

            (await CreateQueryRepo().StateSoftDeleteValidation(1)).Should().BeTrue();
        }

        [Fact]
        public async Task CitySoftDeleteValidation_Should_Return_True_When_Linked()
        {
            await ClearTablesAsync();
            var typeId = await SeedMfgTypeAsync("MFGQ_T7");
            await SeedEntityAsync(typeId);

            (await CreateQueryRepo().CitySoftDeleteValidation(1)).Should().BeTrue();
        }

        [Fact]
        public async Task IsManufactureLinkedAsync_Should_Reflect_Validation_Result()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo(active: true).IsManufactureLinkedAsync(1)).Should().BeTrue();
            (await CreateQueryRepo(active: false).IsManufactureLinkedAsync(1)).Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Reflect_Validation_Result()
        {
            await ClearTablesAsync();

            (await CreateQueryRepo(linked: true).SoftDeleteValidationAsync(1)).Should().BeTrue();
            (await CreateQueryRepo(linked: false).SoftDeleteValidationAsync(1)).Should().BeFalse();
        }
    }
}
