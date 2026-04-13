using Microsoft.EntityFrameworkCore;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.Manufacture;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;

namespace FixedAssetManagement.IntegrationTests.Repositories.Manufacture
{
    [Collection("DatabaseCollection")]
    public sealed class ManufactureCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ManufactureCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ManufactureCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedManufactureTypeAsync(string typeCode = "MFG_T")
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

        private static Manufactures BuildEntity(int? mfgType = null, string code = "MFG001", string name = "Test Manufacturer") =>
            new Manufactures
            {
                Code = code,
                ManufactureName = name,
                ManufactureType = mfgType,
                CountryId = 1,
                StateId = 1,
                CityId = 1,
                AddressLine1 = "Addr 1",
                AddressLine2 = "Addr 2",
                PinCode = "123456",
                PersonName = "Test Person",
                PhoneNumber = "1234567890",
                Email = "test@example.com",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_Entity_With_Id()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedManufactureTypeAsync("MFG_C1");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId));

            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedManufactureTypeAsync("MFG_C2");

            var result = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, "MFG_P", "Acme Corp"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.Manufactures.FirstAsync(x => x.Id == result.Id);
            saved.Code.Should().Be("MFG_P");
            saved.ManufactureName.Should().Be("Acme Corp");
            saved.ManufactureType.Should().Be(typeId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedManufactureTypeAsync("MFG_U1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, "MFG_U", "Original"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Manufactures
            {
                Id = created.Id,
                Code = "MFG_U",
                ManufactureName = "Renamed",
                ManufactureType = typeId,
                CountryId = 1,
                StateId = 1,
                CityId = 1,
                AddressLine1 = "New Addr",
                AddressLine2 = "New Addr2",
                PinCode = "654321",
                PersonName = "Updated Person",
                PhoneNumber = "9876543210",
                Email = "updated@example.com",
                IsActive = BaseEntity.Status.Active
            });
            ctx.ChangeTracker.Clear();

            (await ctx.Manufactures.FirstAsync(x => x.Id == created.Id)).ManufactureName.Should().Be("Renamed");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).UpdateAsync(new Manufactures { Id = 9999, ManufactureName = "X" });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedManufactureTypeAsync("MFG_D1");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).DeleteAsync(created.Id, new Manufactures { IsDeleted = BaseEntity.IsDelete.Deleted });
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.Manufactures.IgnoreQueryFilters().FirstAsync(x => x.Id == created.Id);
            deleted.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).DeleteAsync(9999, new Manufactures { IsDeleted = BaseEntity.IsDelete.Deleted });

            result.Should().Be(0);
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedManufactureTypeAsync("MFG_E1");
            await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, "MFG_EX"));

            (await CreateRepository(ctx).ExistsByCodeAsync("MFG_EX")).Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByCodeAsync_Should_Return_False_When_Excluded()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedManufactureTypeAsync("MFG_E2");
            var created = await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, "MFG_EX2"));

            (await CreateRepository(ctx).ExistsByCodeAsync("MFG_EX2", created.Id)).Should().BeFalse();
        }

        [Fact]
        public async Task ExistsByNameAsync_Should_Return_True_When_Active_Match()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = await SeedManufactureTypeAsync("MFG_E3");
            await CreateRepository(ctx).CreateAsync(BuildEntity(typeId, "MFG_EN", "ActiveMfg"));

            (await CreateRepository(ctx).ExistsByNameAsync("ActiveMfg")).Should().BeTrue();
        }
    }
}
