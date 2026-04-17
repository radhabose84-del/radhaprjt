using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Domain.Common;
using FinanceManagement.Domain.Entities;
using FinanceManagement.Infrastructure.Repositories.Lookups.Finance;
using FinanceManagement.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FinanceManagement.IntegrationTests.Repositories.Lookups
{
    [Collection("DatabaseCollection")]
    public sealed class DocumentSequenceLookupRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DocumentSequenceLookupRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DocumentSequenceLookupRepository CreateRepo(
            List<ModuleLookupDto>? modules = null,
            List<FinancialYearLookupDto>? years = null)
        {
            var conn = new SqlConnection(_fixture.ConnectionString);

            var moduleMock = new Mock<IModuleLookup>(MockBehavior.Loose);
            moduleMock.Setup(m => m.GetAllModuleAsync())
                .ReturnsAsync(modules ?? new List<ModuleLookupDto>
                {
                    new() { ModuleId = 1, ModuleName = "Sales" }
                });

            var yearMock = new Mock<IFinancialYearLookup>(MockBehavior.Loose);
            yearMock.Setup(y => y.GetAllFinancialYearAsync())
                .ReturnsAsync(years ?? new List<FinancialYearLookupDto>
                {
                    new() { FinancialYearId = 10, FinancialYearName = "2024-2025" }
                });

            return new DocumentSequenceLookupRepository(conn, yearMock.Object, moduleMock.Object);
        }

        private async Task<int> SeedTransactionTypeAsync(
            string typeName = "Sales Invoice",
            string shortName = "",
            int unitId = 1,
            int moduleId = 1,
            int menuId = 1,
            BaseEntity.Status active = BaseEntity.Status.Active,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            // Derive a unique short name when not specified to avoid (UnitId, ShortName) unique-index collisions
            var rawShort = typeName.Replace(" ", "");
            var derivedShort = string.IsNullOrEmpty(shortName)
                ? rawShort.Substring(0, Math.Min(rawShort.Length, 8))
                : shortName;
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new FinanceManagement.Domain.Entities.TransactionTypeMaster
            {
                UnitId = unitId, ModuleId = moduleId, MenuId = menuId,
                TypeName = typeName, ShortName = derivedShort, Description = typeName,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.TransactionTypeMaster.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task<int> SeedDocumentSequenceAsync(
            int transactionTypeId,
            int financialYearId = 10,
            int docNo = 1,
            BaseEntity.IsDelete deleted = BaseEntity.IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = new FinanceManagement.Domain.Entities.DocumentSequence
            {
                TransactionTypeId = transactionTypeId,
                FinancialYearId = financialYearId,
                DocNo = docNo,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = deleted
            };
            await ctx.DocumentSequence.AddAsync(entity);
            await ctx.SaveChangesAsync();
            return entity.Id;
        }

        private async Task ClearAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var ds = await ctx.DocumentSequence.ToListAsync();
            ctx.DocumentSequence.RemoveRange(ds);
            await ctx.SaveChangesAsync();

            var tt = await ctx.TransactionTypeMaster.ToListAsync();
            ctx.TransactionTypeMaster.RemoveRange(tt);
            await ctx.SaveChangesAsync();
        }

        // --- GetTransactionTypeIdAsync ---

        [Fact]
        public async Task GetTransactionTypeIdAsync_Should_Return_Matching_Id()
        {
            await ClearAsync();
            var id = await SeedTransactionTypeAsync("Sales Invoice", "SI", unitId: 1, moduleId: 1);

            var result = await CreateRepo().GetTransactionTypeIdAsync("Sales Invoice", "Sales", 1);

            result.Should().Be(id);
        }

        [Fact]
        public async Task GetTransactionTypeIdAsync_Should_Return_Null_When_Module_NotFound()
        {
            var result = await CreateRepo().GetTransactionTypeIdAsync("Sales Invoice", "UnknownModule", 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTransactionTypeIdAsync_Should_Return_Null_When_TypeName_NotFound()
        {
            await ClearAsync();

            var result = await CreateRepo().GetTransactionTypeIdAsync("NoSuchType", "Sales", 1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetTransactionTypeIdAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedTransactionTypeAsync("Del Invoice", "DI", deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().GetTransactionTypeIdAsync("Del Invoice", "Sales", 1);

            result.Should().BeNull();
        }

        // --- GenerateDocumentNumber ---

        [Fact]
        public async Task GenerateDocumentNumber_Should_Return_Formatted_Strings()
        {
            await ClearAsync();
            var ttId = await SeedTransactionTypeAsync(shortName: "SI");
            await SeedDocumentSequenceAsync(ttId, financialYearId: 10, docNo: 42);

            var result = await CreateRepo().GenerateDocumentNumber(ttId);

            result.Should().HaveCount(1);
            result[0].Should().Be("SI/2024/0042");
        }

        [Fact]
        public async Task GenerateDocumentNumber_Should_Return_Empty_When_NoSequences()
        {
            await ClearAsync();
            var ttId = await SeedTransactionTypeAsync();

            var result = await CreateRepo().GenerateDocumentNumber(ttId);

            result.Should().BeEmpty();
        }

        // --- GetTransactionTypesForYearAsync ---

        [Fact]
        public async Task GetTransactionTypesForYearAsync_Should_Return_Types_Joined_With_DocSeq()
        {
            await ClearAsync();
            var ttId = await SeedTransactionTypeAsync("Sales Invoice", "SI");
            await SeedDocumentSequenceAsync(ttId, financialYearId: 10);

            var result = await CreateRepo().GetTransactionTypesForYearAsync(
                financialYearId: 10, unitId: 1);

            result.Should().ContainSingle();
            result[0].TypeName.Should().Be("Sales Invoice");
        }

        [Fact]
        public async Task GetTransactionTypesForYearAsync_Should_Filter_By_UnitId()
        {
            await ClearAsync();
            var ttId1 = await SeedTransactionTypeAsync("U1", unitId: 1);
            var ttId2 = await SeedTransactionTypeAsync("U2", unitId: 2);
            await SeedDocumentSequenceAsync(ttId1);
            await SeedDocumentSequenceAsync(ttId2);

            var result = await CreateRepo().GetTransactionTypesForYearAsync(
                financialYearId: 10, unitId: 1);

            result.Should().ContainSingle(r => r.TypeName == "U1");
        }

        [Fact]
        public async Task GetTransactionTypesForYearAsync_Should_Filter_By_ModuleId()
        {
            await ClearAsync();
            var ttId1 = await SeedTransactionTypeAsync("M1", moduleId: 1);
            var ttId2 = await SeedTransactionTypeAsync("M2", moduleId: 2);
            await SeedDocumentSequenceAsync(ttId1);
            await SeedDocumentSequenceAsync(ttId2);

            var result = await CreateRepo().GetTransactionTypesForYearAsync(
                financialYearId: 10, unitId: 1, moduleId: 2);

            result.Should().ContainSingle(r => r.TypeName == "M2");
        }

        // --- IncrementDocNoAsync ---

        [Fact]
        public async Task IncrementDocNoAsync_Should_Increment_DocNo()
        {
            await ClearAsync();
            var ttId = await SeedTransactionTypeAsync();
            var dsId = await SeedDocumentSequenceAsync(ttId, docNo: 5);

            using var conn = new SqlConnection(_fixture.ConnectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();
            await CreateRepo().IncrementDocNoAsync(ttId, conn, tx);
            tx.Commit();

            await using var ctx = _fixture.CreateFreshDbContext();
            var updated = await ctx.DocumentSequence.AsNoTracking().FirstAsync(d => d.Id == dsId);
            updated.DocNo.Should().Be(6);
        }

        // --- GetTransactionTypesByIdsAsync ---

        [Fact]
        public async Task GetTransactionTypesByIdsAsync_Should_Return_Matching_Records()
        {
            await ClearAsync();
            var id1 = await SeedTransactionTypeAsync("T1");
            var id2 = await SeedTransactionTypeAsync("T2");

            var result = await CreateRepo().GetTransactionTypesByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetTransactionTypesByIdsAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            var id1 = await SeedTransactionTypeAsync("A");
            var id2 = await SeedTransactionTypeAsync("B", deleted: BaseEntity.IsDelete.Deleted);

            var result = await CreateRepo().GetTransactionTypesByIdsAsync(new[] { id1, id2 });

            result.Should().HaveCount(1);
            result[0].Id.Should().Be(id1);
        }
    }
}
