using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.TransactionTypeMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.TransactionTypeMaster
{
    [Collection("DatabaseCollection")]
    public sealed class TransactionTypeMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TransactionTypeMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private TransactionTypeMasterQueryRepository CreateQueryRepo(
            Mock<IUnitLookup>? unitLookup = null,
            Mock<IModuleLookup>? moduleLookup = null,
            Mock<IMenuLookup>? menuLookup = null,
            int? tokenUnitId = 1)
        {
            unitLookup ??= BuildDefaultUnitLookup();
            moduleLookup ??= BuildDefaultModuleLookup();
            menuLookup ??= BuildDefaultMenuLookup();

            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUnitId()).Returns(tokenUnitId);

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new TransactionTypeMasterQueryRepository(
                conn, unitLookup.Object, moduleLookup.Object, menuLookup.Object, ip.Object);
        }

        private static Mock<IUnitLookup> BuildDefaultUnitLookup(int unitId = 1, string unitName = "Test Unit", string shortName = "TU")
        {
            var mock = new Mock<IUnitLookup>(MockBehavior.Loose);
            mock.Setup(u => u.GetAllUnitAsync())
                .ReturnsAsync(new List<UnitLookupDto>
                {
                    new() { UnitId = unitId, UnitName = unitName, ShortName = shortName }
                });
            return mock;
        }

        private static Mock<IModuleLookup> BuildDefaultModuleLookup(int moduleId = 1, string moduleName = "Test Module")
        {
            var mock = new Mock<IModuleLookup>(MockBehavior.Loose);
            mock.Setup(m => m.GetAllModuleAsync())
                .ReturnsAsync(new List<ModuleLookupDto>
                {
                    new() { ModuleId = moduleId, ModuleName = moduleName }
                });
            return mock;
        }

        private static Mock<IMenuLookup> BuildDefaultMenuLookup(int menuId = 1, string menuName = "Test Menu")
        {
            var mock = new Mock<IMenuLookup>(MockBehavior.Loose);
            mock.Setup(m => m.GetAllMenuAsync())
                .ReturnsAsync(new List<MenuLookupDto>
                {
                    new() { MenuId = menuId, MenuName = menuName }
                });
            return mock;
        }

        private async Task<int> SeedEntityAsync(
            string typeName = "Invoice",
            string shortName = "INV",
            string description = "Invoice Type",
            int unitId = 1,
            int moduleId = 1,
            int menuId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUnitId()).Returns(unitId);
            var repo = new TransactionTypeMasterCommandRepository(ctx, ip.Object);
            return await repo.CreateAsync(new Domain.Entities.TransactionTypeMaster
            {
                UnitId = unitId,
                ModuleId = moduleId,
                MenuId = menuId,
                TypeName = typeName,
                ShortName = shortName,
                Description = description,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() =>
            await _fixture.ClearAllTablesAsync();

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
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUnitId()).Returns(1);
            await new TransactionTypeMasterCommandRepository(ctx, ip.Object).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Invoice", "INV");
            await SeedEntityAsync("Payment", "PAY");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Invoice");

            items.Should().HaveCount(1);
            items[0].TypeName.Should().Be("Invoice");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_LookupNames()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var unitMock = BuildDefaultUnitLookup(1, "Main Unit", "MU");
            var moduleMock = BuildDefaultModuleLookup(1, "Finance Module");
            var menuMock = BuildDefaultMenuLookup(1, "Finance Menu");

            var (items, _) = await CreateQueryRepo(unitMock, moduleMock, menuMock).GetAllAsync(1, 10, null);

            items[0].UnitName.Should().Be("Main Unit");
            items[0].ModuleName.Should().Be("Finance Module");
            items[0].MenuName.Should().Be("Finance Menu");
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Only_CurrentUnit_Records()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Invoice1", "INV1", unitId: 1);
            await SeedEntityAsync("Invoice2", "INV2", unitId: 1);
            await SeedEntityAsync("Invoice3", "INV3", unitId: 2);

            var (items, total) = await CreateQueryRepo(tokenUnitId: 1).GetAllAsync(1, 10, null);

            items.Should().HaveCount(2);
            total.Should().Be(2);
            items.Should().OnlyContain(i => i.UnitId == 1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_When_NoUnitClaim()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Invoice1", "INV1", unitId: 1);
            await SeedEntityAsync("Invoice2", "INV2", unitId: 2);

            var (items, total) = await CreateQueryRepo(tokenUnitId: null).GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Invoice", "INV", "Invoice Type");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.TypeName.Should().Be("Invoice");
            dto.ShortName.Should().Be("INV");
            dto.Description.Should().Be("Invoice Type");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUnitId()).Returns(1);
            await new TransactionTypeMasterCommandRepository(ctx, ip.Object).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_DifferentUnit()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(unitId: 2);

            var dto = await CreateQueryRepo(tokenUnitId: 1).GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Invoice", "INV");
            await SeedEntityAsync("Payment", "PAY");

            var results = await CreateQueryRepo().AutocompleteAsync("Inv", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].TypeName.Should().Be("Invoice");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Invoice", "INV");
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.TransactionTypeMaster.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Invoice", CancellationToken.None);

            results.Should().BeEmpty();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_OtherUnits()
        {
            await ClearTableAsync();
            await SeedEntityAsync("InvoiceU1", "IU1", unitId: 1);
            await SeedEntityAsync("InvoiceU2", "IU2", unitId: 2);

            var results = await CreateQueryRepo(tokenUnitId: 1).AutocompleteAsync("Invoice", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].TypeName.Should().Be("InvoiceU1");
        }

        // --- TYPE NAME EXISTS ---

        [Fact]
        public async Task TypeNameExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Invoice", "INV");

            var exists = await CreateQueryRepo().TypeNameExistsAsync("Invoice", 1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task TypeNameExistsAsync_Should_Return_False_After_SoftDelete()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Invoice", "INV");
            await using var ctx = _fixture.CreateFreshDbContext();
            var ip = new Mock<IIPAddressService>(MockBehavior.Loose);
            ip.Setup(x => x.GetUnitId()).Returns(1);
            await new TransactionTypeMasterCommandRepository(ctx, ip.Object).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().TypeNameExistsAsync("Invoice", 1);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task TypeNameExistsAsync_Should_Return_False_For_SameName_In_DifferentUnit()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Invoice", "INV", unitId: 2);

            var exists = await CreateQueryRepo().TypeNameExistsAsync("Invoice", 1);

            exists.Should().BeFalse();
        }

        // --- SHORT NAME EXISTS ---

        [Fact]
        public async Task ShortNameExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Invoice", "INV");

            var exists = await CreateQueryRepo().ShortNameExistsAsync("INV", 1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ShortNameExistsAsync_Should_Return_False_For_SameShort_In_DifferentUnit()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Invoice", "INV", unitId: 2);

            var exists = await CreateQueryRepo().ShortNameExistsAsync("INV", 1);

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
        public async Task NotFoundAsync_Should_Return_True_When_Exists_In_OtherUnit()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync(unitId: 2);

            var notFound = await CreateQueryRepo(tokenUnitId: 1).NotFoundAsync(id);

            notFound.Should().BeTrue();
        }

        // --- UNIT/MODULE/MENU EXISTS (lookup-based) ---

        [Fact]
        public async Task UnitExistsAsync_Should_Return_True_When_Lookup_Has_Match()
        {
            var unitMock = BuildDefaultUnitLookup(1, "Main Unit");
            var exists = await CreateQueryRepo(unitMock).UnitExistsAsync(1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task UnitExistsAsync_Should_Return_False_When_Lookup_Has_No_Match()
        {
            var unitMock = BuildDefaultUnitLookup(1, "Main Unit");
            var exists = await CreateQueryRepo(unitMock).UnitExistsAsync(999);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task ModuleExistsAsync_Should_Return_True_When_Lookup_Has_Match()
        {
            var moduleMock = BuildDefaultModuleLookup(1, "Finance");
            var exists = await CreateQueryRepo(moduleLookup: moduleMock).ModuleExistsAsync(1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ModuleExistsAsync_Should_Return_False_When_Lookup_Has_No_Match()
        {
            var moduleMock = BuildDefaultModuleLookup(1, "Finance");
            var exists = await CreateQueryRepo(moduleLookup: moduleMock).ModuleExistsAsync(999);

            exists.Should().BeFalse();
        }

        [Fact]
        public async Task MenuExistsAsync_Should_Return_True_When_Lookup_Has_Match()
        {
            var menuMock = BuildDefaultMenuLookup(1, "Finance Menu");
            var exists = await CreateQueryRepo(menuLookup: menuMock).MenuExistsAsync(1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task MenuExistsAsync_Should_Return_False_When_Lookup_Has_No_Match()
        {
            var menuMock = BuildDefaultMenuLookup(1, "Finance Menu");
            var exists = await CreateQueryRepo(menuLookup: menuMock).MenuExistsAsync(999);

            exists.Should().BeFalse();
        }
    }
}
