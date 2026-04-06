using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
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
            Mock<IMenuLookup>? menuLookup = null)
        {
            unitLookup ??= BuildDefaultUnitLookup();
            moduleLookup ??= BuildDefaultModuleLookup();
            menuLookup ??= BuildDefaultMenuLookup();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new TransactionTypeMasterQueryRepository(conn, unitLookup.Object, moduleLookup.Object, menuLookup.Object);
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
            var repo = new TransactionTypeMasterCommandRepository(ctx);
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

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Finance].[DocumentSequence]");
            await conn.ExecuteAsync("DELETE FROM [Finance].[TransactionTypeMaster]");
        }

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
            await new TransactionTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

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
            await new TransactionTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

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

        // --- TYPE NAME EXISTS ---

        [Fact]
        public async Task TypeNameExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Invoice", "INV");

            var exists = await CreateQueryRepo().TypeNameExistsAsync("Invoice");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task TypeNameExistsAsync_Should_Return_False_After_SoftDelete()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("Invoice", "INV");
            await using var ctx = _fixture.CreateFreshDbContext();
            await new TransactionTypeMasterCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var exists = await CreateQueryRepo().TypeNameExistsAsync("Invoice");

            exists.Should().BeFalse();
        }

        // --- SHORT NAME EXISTS ---

        [Fact]
        public async Task ShortNameExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("Invoice", "INV");

            var exists = await CreateQueryRepo().ShortNameExistsAsync("INV");

            exists.Should().BeTrue();
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
