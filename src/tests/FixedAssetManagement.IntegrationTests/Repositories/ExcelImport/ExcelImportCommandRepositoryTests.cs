using Contracts.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FAM.Application.Common.Interfaces.ILocation;
using FAM.Application.Common.Interfaces.ISubLocation;
using FAM.Domain.Common;
using FAM.Domain.Entities;
using FAM.Infrastructure.Repositories.AssetCategories;
using FAM.Infrastructure.Repositories.AssetGroup;
using FAM.Infrastructure.Repositories.AssetSubCategories;
using FAM.Infrastructure.Repositories.AssetSubGroup;
using FAM.Infrastructure.Repositories.ExcelImport;
using FAM.Infrastructure.Repositories.Locations;
using FAM.Infrastructure.Repositories.Manufacture;
using FAM.Infrastructure.Repositories.MiscMaster;
using FAM.Infrastructure.Repositories.MiscTypeMaster;
using FAM.Infrastructure.Repositories.SubLocation;
using FAM.Infrastructure.Repositories.UOMs;

namespace FixedAssetManagement.IntegrationTests.Repositories.ExcelImport
{
    [Collection("DatabaseCollection")]
    public sealed class ExcelImportCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ExcelImportCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ExcelImportCommandRepository CreateRepository(FAM.Infrastructure.Data.ApplicationDbContext ctx)
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(1);
            ipMock.Setup(x => x.GetUserId()).Returns(1);
            ipMock.Setup(x => x.GetUserName()).Returns("test-user");
            ipMock.Setup(x => x.GetSystemIPAddress()).Returns("127.0.0.1");
            var mediator = new Mock<IMediator>(MockBehavior.Loose);
            ILocationCommandRepository locRepo = new LocationCommandRepository(ctx);
            ISubLocationCommandRepository subRepo = new SubLocationCommandRepository(ctx);
            return new ExcelImportCommandRepository(ctx, mediator.Object, locRepo, ipMock.Object, subRepo);
        }

        private async Task ClearTablesAsync(FAM.Infrastructure.Data.ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAssetGroupIdByNameAsync_Should_Return_Id_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await new AssetGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = "EI_AG1",
                GroupName = "ExcelGroup",
                GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            var result = await CreateRepository(ctx).GetAssetGroupIdByNameAsync("ExcelGroup");

            result.Should().Be(groupId);
        }

        [Fact]
        public async Task GetAssetGroupIdByNameAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).GetAssetGroupIdByNameAsync("NoSuch");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAssetSubGroupIdByNameAsync_Should_Return_Id_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await new AssetGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = "EI_AG2", GroupName = "G2", GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            var subId = await new AssetSubGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetSubGroup
            {
                Code = "EI_SG1", SubGroupName = "ExcelSubGroup", GroupId = groupId,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            var result = await CreateRepository(ctx).GetAssetSubGroupIdByNameAsync("ExcelSubGroup");

            result.Should().Be(subId);
        }

        [Fact]
        public async Task GetAssetCategoryIdByNameAsync_Should_Return_Id_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await new AssetGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = "EI_AG3", GroupName = "G3", GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            var catId = await new AssetCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetCategories
            {
                Code = "EI_C1", CategoryName = "ExcelCat", AssetGroupId = groupId,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            var result = await CreateRepository(ctx).GetAssetCategoryIdByNameAsync("ExcelCat");

            result.Should().Be(catId);
        }

        [Fact]
        public async Task GetAssetSubCategoryIdByNameAsync_Should_Return_Id_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var groupId = await new AssetGroupCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetGroup
            {
                Code = "EI_AG4", GroupName = "G4", GroupPercentage = 10m,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            var catId = await new AssetCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetCategories
            {
                Code = "EI_C2", CategoryName = "C2", AssetGroupId = groupId,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            var subCatId = await new AssetSubCategoriesCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.AssetSubCategories
            {
                Code = "EI_SC1", SubCategoryName = "ExcelSubCat", AssetCategoriesId = catId,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            var result = await CreateRepository(ctx).GetAssetSubCategoryIdByNameAsync(catId, "ExcelSubCat");

            result.Should().Be(subCatId);
        }

        [Fact]
        public async Task GetAssetUOMIdByNameAsync_Should_Return_Id_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var typeId = (await new MiscTypeMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = "EI_T1", Description = "T",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
            var miscId = (await new MiscMasterCommandRepository(ctx).CreateAsync(new FAM.Domain.Entities.MiscMaster
            {
                MiscTypeId = typeId, Code = "MM_EI_T1", Description = "M",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            })).Id;
            var uom = await new UOMCommandRepository(ctx).CreateAsync(new UOM
            {
                Code = "EI_U1", UOMName = "ExcelUOM", UOMTypeId = miscId,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            var result = await CreateRepository(ctx).GetAssetUOMIdByNameAsync("ExcelUOM");

            result.Should().Be(uom.Id);
        }

        [Fact]
        public async Task GetAssetLocationIdByNameAsync_Should_Return_Existing_Location()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var loc = await new LocationCommandRepository(ctx).CreateAsync(new Location
            {
                Code = "EI_L1", LocationName = "ExcelLoc", UnitId = 1, DepartmentId = 1,
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            var result = await CreateRepository(ctx).GetAssetLocationIdByNameAsync("ExcelLoc");

            result.Should().Be(loc.Id);
        }

        [Fact]
        public async Task GetAssetLocationIdByNameAsync_Should_Return_Null_When_Empty()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).GetAssetLocationIdByNameAsync(string.Empty);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAssetIdByNameAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).GetAssetIdByNameAsync("NOSUCH");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetManufacturerIdByNameAsync_Should_Return_Id_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var mfg = await new ManufactureCommandRepository(ctx).CreateAsync(new Manufactures
            {
                Code = "EI_M1", ManufactureName = "ExcelMfg",
                CountryId = 1, StateId = 1, CityId = 1,
                AddressLine1 = "A", AddressLine2 = "B",
                PinCode = "123456", PersonName = "Test", PhoneNumber = "1234567890", Email = "t@t.com",
                IsActive = BaseEntity.Status.Active, IsDeleted = BaseEntity.IsDelete.NotDeleted
            });

            var result = await CreateRepository(ctx).GetManufacturerIdByNameAsync("ExcelMfg");

            result.Should().Be(mfg.Id);
        }

        [Fact]
        public async Task CheckFileExistsAsync_Should_Return_False_When_File_NotInDb()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM [FixedAsset].[AssetAudit]");

            var result = await CreateRepository(ctx).CheckFileExistsAsync("nofile.xlsx", CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
