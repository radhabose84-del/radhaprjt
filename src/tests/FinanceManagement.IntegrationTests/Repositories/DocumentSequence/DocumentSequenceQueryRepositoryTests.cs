using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Infrastructure.Data;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.DocumentSequence;
using FinanceManagement.Infrastructure.Repositories.TransactionTypeMaster;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.DocumentSequence
{
    [Collection("DatabaseCollection")]
    public sealed class DocumentSequenceQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DocumentSequenceQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DocumentSequenceQueryRepository CreateQueryRepo(
            Mock<IUnitLookup>? unitLookup = null,
            Mock<IFinancialYearLookup>? financialYearLookup = null,
            Mock<IModuleLookup>? moduleLookup = null)
        {
            unitLookup ??= BuildDefaultUnitLookup();
            financialYearLookup ??= BuildDefaultFinancialYearLookup();
            moduleLookup ??= BuildDefaultModuleLookup();

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new DocumentSequenceQueryRepository(conn, unitLookup.Object, financialYearLookup.Object, moduleLookup.Object);
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

        private static Mock<IFinancialYearLookup> BuildDefaultFinancialYearLookup(int fyId = 1, string fyName = "2025-2026")
        {
            var mock = new Mock<IFinancialYearLookup>(MockBehavior.Loose);
            mock.Setup(f => f.GetAllFinancialYearAsync())
                .ReturnsAsync(new List<FinancialYearLookupDto>
                {
                    new() { FinancialYearId = fyId, FinancialYearName = fyName }
                });
            return mock;
        }

        private static Mock<IModuleLookup> BuildDefaultModuleLookup(int moduleId = 1, string moduleName = "Finance")
        {
            var mock = new Mock<IModuleLookup>(MockBehavior.Loose);
            mock.Setup(m => m.GetAllModuleAsync())
                .ReturnsAsync(new List<ModuleLookupDto>
                {
                    new() { ModuleId = moduleId, ModuleName = moduleName }
                });
            return mock;
        }

        private async Task<int> SeedTransactionTypeAsync(
            string typeName = "Invoice",
            string shortName = "INV",
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
                Description = "Test Type",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task<int> SeedDocumentSequenceAsync(int transactionTypeId, int financialYearId = 1, int docNo = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new DocumentSequenceCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.DocumentSequence
            {
                TransactionTypeId = transactionTypeId,
                FinancialYearId = financialYearId,
                DocNo = docNo,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTablesAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync();
            await SeedDocumentSequenceAsync(typeId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync();
            var id = await SeedDocumentSequenceAsync(typeId);
            await using var ctx = _fixture.CreateFreshDbContext();
            await new DocumentSequenceCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTablesAsync();
            var typeId1 = await SeedTransactionTypeAsync("Invoice", "INV");
            var typeId2 = await SeedTransactionTypeAsync("Payment", "PAY");
            await SeedDocumentSequenceAsync(typeId1, docNo: 1);
            await SeedDocumentSequenceAsync(typeId2, docNo: 2);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, "Invoice");

            items.Should().HaveCount(1);
            items[0].TypeName.Should().Be("Invoice");
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_LookupNames()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync();
            await SeedDocumentSequenceAsync(typeId);

            var unitMock = BuildDefaultUnitLookup(1, "Main Unit", "MU");
            var fyMock = BuildDefaultFinancialYearLookup(1, "2025-2026");

            var (items, _) = await CreateQueryRepo(unitMock, fyMock).GetAllAsync(1, 10, null);

            items[0].UnitShortName.Should().Be("MU");
            items[0].FinancialYearName.Should().Be("2025-2026");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync("Invoice", "INV");
            var id = await SeedDocumentSequenceAsync(typeId, financialYearId: 1, docNo: 42);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.TransactionTypeId.Should().Be(typeId);
            dto.TypeName.Should().Be("Invoice");
            dto.DocNo.Should().Be(42);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync();
            var id = await SeedDocumentSequenceAsync(typeId);
            await using var ctx = _fixture.CreateFreshDbContext();
            await new DocumentSequenceCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Matching()
        {
            await ClearTablesAsync();
            var typeId1 = await SeedTransactionTypeAsync("Invoice", "INV");
            var typeId2 = await SeedTransactionTypeAsync("Payment", "PAY");
            await SeedDocumentSequenceAsync(typeId1, docNo: 1);
            await SeedDocumentSequenceAsync(typeId2, docNo: 2);

            var results = await CreateQueryRepo().AutocompleteAsync("Inv", CancellationToken.None);

            results.Should().HaveCount(1);
            results[0].TypeName.Should().Be("Invoice");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync("Invoice", "INV");
            var id = await SeedDocumentSequenceAsync(typeId);
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.DocumentSequence.FirstAsync(x => x.Id == id);
            entity.IsActive = Status.Inactive;
            await ctx.SaveChangesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("Invoice", CancellationToken.None);

            results.Should().BeEmpty();
        }

        // --- COMPOSITE KEY EXISTS ---

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync();
            await SeedDocumentSequenceAsync(typeId, financialYearId: 1, docNo: 100);

            var exists = await CreateQueryRepo().CompositeKeyExistsAsync(typeId, 1, 100);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task CompositeKeyExistsAsync_Should_Return_False_When_No_Duplicate()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync();
            await SeedDocumentSequenceAsync(typeId, financialYearId: 1, docNo: 100);

            var exists = await CreateQueryRepo().CompositeKeyExistsAsync(typeId, 1, 999);

            exists.Should().BeFalse();
        }

        // --- TRANSACTION TYPE EXISTS (same-module, direct SQL) ---

        [Fact]
        public async Task TransactionTypeIdExistsAsync_Should_Return_True_When_Exists()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync();

            var exists = await CreateQueryRepo().TransactionTypeIdExistsAsync(typeId);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task TransactionTypeIdExistsAsync_Should_Return_False_When_NotExists()
        {
            await ClearTablesAsync();

            var exists = await CreateQueryRepo().TransactionTypeIdExistsAsync(9999);

            exists.Should().BeFalse();
        }

        // --- FINANCIAL YEAR EXISTS (lookup-based) ---

        [Fact]
        public async Task FinancialYearExistsAsync_Should_Return_True_When_Lookup_Has_Match()
        {
            var fyMock = BuildDefaultFinancialYearLookup(1, "2025-2026");
            var exists = await CreateQueryRepo(financialYearLookup: fyMock).FinancialYearExistsAsync(1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task FinancialYearExistsAsync_Should_Return_False_When_Lookup_Has_No_Match()
        {
            var fyMock = BuildDefaultFinancialYearLookup(1, "2025-2026");
            var exists = await CreateQueryRepo(financialYearLookup: fyMock).FinancialYearExistsAsync(999);

            exists.Should().BeFalse();
        }

        // --- NOT FOUND ---

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_NotExists()
        {
            await ClearTablesAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync();
            var id = await SeedDocumentSequenceAsync(typeId);

            var notFound = await CreateQueryRepo().NotFoundAsync(id);

            notFound.Should().BeFalse();
        }

        // --- GET TRANSACTION TYPE ID ---

        [Fact]
        public async Task GetTransactionTypeIdAsync_Should_Return_Correct_Id()
        {
            await ClearTablesAsync();
            var typeId = await SeedTransactionTypeAsync("Invoice", "INV", unitId: 1, moduleId: 1);

            var moduleMock = BuildDefaultModuleLookup(1, "Finance");
            var result = await CreateQueryRepo(moduleLookup: moduleMock).GetTransactionTypeIdAsync("Invoice", "Finance", 1);

            result.Should().Be(typeId);
        }

        [Fact]
        public async Task GetTransactionTypeIdAsync_Should_Return_Null_When_Module_Not_Found()
        {
            await ClearTablesAsync();
            await SeedTransactionTypeAsync("Invoice", "INV", unitId: 1, moduleId: 1);

            var moduleMock = BuildDefaultModuleLookup(99, "NonExistent");
            var result = await CreateQueryRepo(moduleLookup: moduleMock).GetTransactionTypeIdAsync("Invoice", "Finance", 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTransactionTypeIdAsync_Should_Return_Null_When_TypeName_Not_Found()
        {
            await ClearTablesAsync();
            await SeedTransactionTypeAsync("Invoice", "INV", unitId: 1, moduleId: 1);

            var moduleMock = BuildDefaultModuleLookup(1, "Finance");
            var result = await CreateQueryRepo(moduleLookup: moduleMock).GetTransactionTypeIdAsync("NonExistent", "Finance", 1);

            result.Should().BeNull();
        }
    }
}
