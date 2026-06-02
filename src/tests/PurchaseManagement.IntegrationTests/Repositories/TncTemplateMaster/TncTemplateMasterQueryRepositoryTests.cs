using Contracts.Dtos.Lookups.Finance;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Moq;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Infrastructure.Repositories.TncTemplateMaster;
using PurchaseManagement.IntegrationTests.Common;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.IntegrationTests.Repositories.TncTemplateMaster
{
    [Collection("DatabaseCollection")]
    public sealed class TncTemplateMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public TncTemplateMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        // ModuleId / TransactionTypeId are cross-module FKs (no DB constraint) — plain ints.
        private const int ModuleId = 1;

        private static Mock<IModuleLookup> BuildModuleLookup() =>
            BuildModuleLookup(new List<ModuleLookupDto>
            {
                new() { ModuleId = ModuleId, ModuleName = "Purchase" }
            });

        private static Mock<IModuleLookup> BuildModuleLookup(List<ModuleLookupDto> modules)
        {
            var mock = new Mock<IModuleLookup>(MockBehavior.Loose);
            mock.Setup(m => m.GetAllModuleAsync()).ReturnsAsync(modules);
            return mock;
        }

        private static Mock<ITransactionTypeLookup> BuildTransactionTypeLookup(
            params TransactionTypeLookupDto[] types)
        {
            var mock = new Mock<ITransactionTypeLookup>(MockBehavior.Loose);
            mock.Setup(t => t.GetAllTransactionTypeAsync())
                .ReturnsAsync(types.ToList());
            mock.Setup(t => t.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync((IEnumerable<int> ids) =>
                {
                    var set = ids.ToHashSet();
                    return types.Where(x => set.Contains(x.Id)).ToList();
                });
            return mock;
        }

        private TncTemplateMasterQueryRepository CreateRepo(
            Mock<IModuleLookup>? moduleLookup = null,
            Mock<ITransactionTypeLookup>? txnLookup = null)
        {
            moduleLookup ??= BuildModuleLookup();
            txnLookup ??= BuildTransactionTypeLookup();
            return new TncTemplateMasterQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                moduleLookup.Object,
                txnLookup.Object);
        }

        private async Task<int> SeedTemplateAsync(
            string templateName,
            string templateCode,
            int moduleId = ModuleId,
            int? transactionTypeId = null,
            Status active = Status.Active,
            IsDelete deleted = IsDelete.NotDeleted)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = new TnCTemplateMaster
            {
                TemplateCode = templateCode,
                TemplateName = templateName,
                ModuleId = moduleId,
                TermsHtml = "<p>Sample</p>",
                IsActive = active,
                IsDeleted = deleted
            };
            await ctx.TnCTemplateMaster.AddAsync(t);
            await ctx.SaveChangesAsync();

            if (transactionTypeId.HasValue)
            {
                await ctx.TnCTemplateApplicability.AddAsync(new TnCTemplateApplicability
                {
                    TnCTemplateMasterId = t.Id,
                    TransactionTypeId = transactionTypeId.Value,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                });
                await ctx.SaveChangesAsync();
            }

            return t.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        // --- GetAllTncTemplateAsync ---

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedTemplateAsync("TQ_All1", "TQ_C1");

            var (rows, total) = await CreateRepo().GetAllTncTemplateAsync(1, 10, null!);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Populate_ModuleName()
        {
            await ClearAsync();
            await SeedTemplateAsync("TQ_Mod", "TQ_MOD1");

            var (rows, _) = await CreateRepo().GetAllTncTemplateAsync(1, 10, null!);

            rows[0].ModuleName.Should().Be("Purchase");
        }

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Populate_TransactionTypeNames()
        {
            await ClearAsync();
            await SeedTemplateAsync("TQ_Txn", "TQ_TXN1", transactionTypeId: 50);

            var txnLookup = BuildTransactionTypeLookup(
                new TransactionTypeLookupDto { Id = 50, TypeName = "Purchase Order", ShortName = "PO" });

            var (rows, _) = await CreateRepo(txnLookup: txnLookup).GetAllTncTemplateAsync(1, 10, null!);

            var app = rows[0].Applicabilities.Should().ContainSingle().Subject;
            app.TransactionTypeId.Should().Be(50);
            app.ShortName.Should().Be("PO");
            app.TypeName.Should().Be("Purchase Order");
        }

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedTemplateAsync("UNIQ_TEMPLATE", "TQ_C2");
            await SeedTemplateAsync("Other_Template", "TQ_C3");

            var (rows, _) = await CreateRepo().GetAllTncTemplateAsync(1, 10, "UNIQ_");

            rows.Should().HaveCount(1);
            rows[0].TemplateName.Should().Be("UNIQ_TEMPLATE");
        }

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedTemplateAsync("TQ_DEL", "TQ_C4", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllTncTemplateAsync(1, 10, null!);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllTncTemplateAsync_Should_Default_Pagination()
        {
            await ClearAsync();
            await SeedTemplateAsync("TQ_PAG", "TQ_PG1");

            var (rows, _) = await CreateRepo().GetAllTncTemplateAsync(0, 0, null!);

            rows.Should().NotBeNull();
        }

        // --- GetByIdAsync ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching_With_Children()
        {
            await ClearAsync();
            var id = await SeedTemplateAsync("TQ_GBI", "TQ_GBI", transactionTypeId: 60);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result.TemplateName.Should().Be("TQ_GBI");
            result.Applicabilities.Should().ContainSingle();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedTemplateAsync("TQ_GSD", "TQ_GSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        // --- ExistsByModuleAndNameAsync ---

        [Fact]
        public async Task ExistsByModuleAndNameAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedTemplateAsync("TQ_DUP", "TQ_DUP", moduleId: ModuleId);

            var result = await CreateRepo().ExistsByModuleAndNameAsync(ModuleId, "TQ_DUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByModuleAndNameAsync_Should_Exclude_Self()
        {
            await ClearAsync();
            var id = await SeedTemplateAsync("TQ_SELF", "TQ_SELF", moduleId: ModuleId);

            var result = await CreateRepo().ExistsByModuleAndNameAsync(ModuleId, "TQ_SELF", excludeId: id);

            result.Should().BeFalse();
        }

        // --- TransactionTypesExistAsync (validated via the cross-module lookup) ---

        [Fact]
        public async Task TransactionTypesExistAsync_Should_Return_False_For_Empty_Input()
        {
            var result = await CreateRepo().TransactionTypesExistAsync(Array.Empty<int>());
            result.Should().BeFalse();
        }

        [Fact]
        public async Task TransactionTypesExistAsync_Should_Return_False_When_Some_Missing()
        {
            var txnLookup = BuildTransactionTypeLookup(
                new TransactionTypeLookupDto { Id = 70, TypeName = "PO", ShortName = "PO" });

            var result = await CreateRepo(txnLookup: txnLookup)
                .TransactionTypesExistAsync(new[] { 70, 9999999 });

            result.Should().BeFalse();
        }

        [Fact]
        public async Task TransactionTypesExistAsync_Should_Return_True_When_All_Present()
        {
            var txnLookup = BuildTransactionTypeLookup(
                new TransactionTypeLookupDto { Id = 80, TypeName = "PO", ShortName = "PO" },
                new TransactionTypeLookupDto { Id = 81, TypeName = "RFQ", ShortName = "RFQ" });

            var result = await CreateRepo(txnLookup: txnLookup)
                .TransactionTypesExistAsync(new[] { 80, 81 });

            result.Should().BeTrue();
        }

        // --- IsUsedInTransactionsAsync ---
        // Skipped: queries Purchase.PurchaseOrder + Sales.SalesOrder which the
        // PurchaseManagement_TestDb does not provision (test DB only contains tables
        // for entities owned by this module). Method is exercised by handler-level tests.

        // --- GetTnCTemplateAutoCompleteAsync ---

        [Fact]
        public async Task GetTnCTemplateAutoCompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedTemplateAsync("AC_Match1", "AC_M1");
            await SeedTemplateAsync("AC_Match2", "AC_M2", active: Status.Inactive);

            var result = await CreateRepo().GetTnCTemplateAutoCompleteAsync("AC_Match", null, null);

            result.Should().HaveCount(1);
            result[0].TemplateName.Should().Be("AC_Match1");
        }

        // --- SoftDeleteValidationAsync ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Children()
        {
            await ClearAsync();
            var id = await SeedTemplateAsync("TQ_SDV1", "TQ_SDV1");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_Children_Exist()
        {
            await ClearAsync();
            var id = await SeedTemplateAsync("TQ_SDV2", "TQ_SDV2", transactionTypeId: 90);

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        // --- IsTnCTemplateLinkedAsync ---

        [Fact]
        public async Task IsTnCTemplateLinkedAsync_Should_Return_False_When_No_Active_Children()
        {
            await ClearAsync();
            var id = await SeedTemplateAsync("TQ_LK1", "TQ_LK1");

            var result = await CreateRepo().IsTnCTemplateLinkedAsync(id);

            result.Should().BeFalse();
        }
    }
}
