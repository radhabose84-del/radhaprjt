using Dapper;
using InventoryManagement.Domain.Common;
using InventoryManagement.Infrastructure.Repositories.MiscMaster;
using InventoryManagement.Infrastructure.Repositories.MiscTypeMaster;
using InventoryManagement.Infrastructure.Repositories.UOMConversion;
using InventoryManagement.Infrastructure.Repositories.UOMs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.IntegrationTests.Repositories.UOMConversion
{
    [Collection("DatabaseCollection")]
    public sealed class UOMConversionQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public UOMConversionQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private UOMConversionQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString));

        private async Task<int> SeedMiscTypeMasterAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "UOMConv Query Misc Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = "UOMConv Query Misc Master",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedUOMAsync(int uomTypeId, string code, string name)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new UOMCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.UOM
            {
                Code = code,
                UOMName = name,
                UOMTypeId = uomTypeId,
                SortOrder = 0,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedConversionAsync(int fromId, int toId, decimal value = 1000m)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new UOMConversionCommandRepository(ctx);
            var result = await repo.CreateAsync(new InventoryManagement.Domain.Entities.UOMConversion
            {
                FromUOMId = fromId,
                ToUOMId = toId,
                ConversionValue = value,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Inventory].[UOMConversion]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[UOM]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Inventory].[MiscTypeMaster]");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UQ_MT_G1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UQ_MM_G1");
            var fromId = await SeedUOMAsync(miscId, "UQ_KG_G1", "Kilogram G1");
            var toId = await SeedUOMAsync(miscId, "UQ_GM_G1", "Gram G1");
            var id = await SeedConversionAsync(fromId, toId, 1000m);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.FromUOMId.Should().Be(fromId);
            result.ToUOMId.Should().Be(toId);
            result.ConversionValue.Should().Be(1000m);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_UOM_Codes()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UQ_MT_G2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UQ_MM_G2");
            var fromId = await SeedUOMAsync(miscId, "UQ_KG_G2", "Kilogram G2");
            var toId = await SeedUOMAsync(miscId, "UQ_GM_G2", "Gram G2");
            var id = await SeedConversionAsync(fromId, toId);

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.FromUOMCode.Should().Be("UQ_KG_G2");
            result.ToUOMCode.Should().Be("UQ_GM_G2");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UQ_MT_G3");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UQ_MM_G3");
            var fromId = await SeedUOMAsync(miscId, "UQ_KG_G3", "Kilogram G3");
            var toId = await SeedUOMAsync(miscId, "UQ_GM_G3", "Gram G3");
            var id = await SeedConversionAsync(fromId, toId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new UOMConversionCommandRepository(ctx).DeleteAsync(id, new InventoryManagement.Domain.Entities.UOMConversion
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            });

            var result = await CreateQueryRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999);

            result.Should().BeNull();
        }

        // --- ALREADY EXISTS ---

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UQ_MT_A1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UQ_MM_A1");
            var fromId = await SeedUOMAsync(miscId, "UQ_KG_A1", "Kilogram A1");
            var toId = await SeedUOMAsync(miscId, "UQ_GM_A1", "Gram A1");
            await SeedConversionAsync(fromId, toId);

            var exists = await CreateQueryRepo().AlreadyExistsAsync(fromId, toId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_When_No_Match()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().AlreadyExistsAsync(9991, 9992);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_False_For_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UQ_MT_A2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UQ_MM_A2");
            var fromId = await SeedUOMAsync(miscId, "UQ_KG_A2", "Kilogram A2");
            var toId = await SeedUOMAsync(miscId, "UQ_GM_A2", "Gram A2");
            var id = await SeedConversionAsync(fromId, toId);

            await using var ctx = _fixture.CreateFreshDbContext();
            await new UOMConversionCommandRepository(ctx).DeleteAsync(id, new InventoryManagement.Domain.Entities.UOMConversion
            {
                IsDeleted = BaseEntity.IsDelete.Deleted
            });

            var exists = await CreateQueryRepo().AlreadyExistsAsync(fromId, toId);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Exclude_Self_When_Id_Provided()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UQ_MT_A3");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UQ_MM_A3");
            var fromId = await SeedUOMAsync(miscId, "UQ_KG_A3", "Kilogram A3");
            var toId = await SeedUOMAsync(miscId, "UQ_GM_A3", "Gram A3");
            var id = await SeedConversionAsync(fromId, toId);

            var exists = await CreateQueryRepo().AlreadyExistsAsync(fromId, toId, id);

            exists.Should().BeFalse();
        }

        // --- GET CONVERSION FACTOR ---

        [Fact]
        public async Task GetConversionFactorAsync_Should_Return_Correct_Value()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UQ_MT_CF1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UQ_MM_CF1");
            var fromId = await SeedUOMAsync(miscId, "UQ_KG_CF1", "Kilogram CF1");
            var toId = await SeedUOMAsync(miscId, "UQ_GM_CF1", "Gram CF1");
            await SeedConversionAsync(fromId, toId, 1000m);

            var factor = await CreateQueryRepo().GetConversionFactorAsync(fromId, toId);

            factor.Should().Be(1000m);
        }

        [Fact]
        public async Task GetConversionFactorAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var factor = await CreateQueryRepo().GetConversionFactorAsync(9991, 9992);

            factor.Should().BeNull();
        }

        [Fact]
        public async Task GetConversionFactorAsync_Should_Return_Null_When_Inactive()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("UQ_MT_CF2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "UQ_MM_CF2");
            var fromId = await SeedUOMAsync(miscId, "UQ_KG_CF2", "Kilogram CF2");
            var toId = await SeedUOMAsync(miscId, "UQ_GM_CF2", "Gram CF2");
            var id = await SeedConversionAsync(fromId, toId, 500m);

            // Inactivate the conversion
            await using var ctx = _fixture.CreateFreshDbContext();
            await new UOMConversionCommandRepository(ctx).UpdateAsync(id, new InventoryManagement.Domain.Entities.UOMConversion
            {
                FromUOMId = fromId,
                ToUOMId = toId,
                ConversionValue = 500m,
                IsActive = BaseEntity.Status.Inactive
            });

            var factor = await CreateQueryRepo().GetConversionFactorAsync(fromId, toId);

            factor.Should().BeNull();
        }
    }
}
