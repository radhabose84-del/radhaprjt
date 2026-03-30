using Dapper;
using Microsoft.Data.SqlClient;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.BankAccount;
using PartyManagement.Infrastructure.Repositories.BankMaster;
using PartyManagement.Infrastructure.Repositories.MiscMaster;
using PartyManagement.Infrastructure.Repositories.MiscTypeMaster;
using Infrastructure.Repositories.Party.BankAccounts;

namespace PartyManagement.IntegrationTests.Repositories.BankAccount
{
    [Collection("DatabaseCollection")]
    public sealed class BankAccountQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BankAccountQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BankAccountQueryRepository CreateQueryRepo()
        {
            var ctx = _fixture.CreateFreshDbContext();
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new BankAccountQueryRepository(ctx, conn);
        }

        private async Task<int> SeedMiscTypeMasterAsync(string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test Misc Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new PartyManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscTypeId,
                Code = code,
                Description = "Test Misc",
                SortOrder = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedBankMasterAsync(string code, string name)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new BankMasterCommandRepository(ctx);
            return await repo.AddAsync(new PartyManagement.Domain.Entities.BankMaster
            {
                BankCode = code,
                BankName = name,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            }, CancellationToken.None);
        }

        private async Task<int> SeedBankAccountAsync(
            int bankId, int accountTypeId, int branchId,
            string accountNumber = "1234567890",
            string holderName = "Test Holder")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new BankAccountCommandRepository(ctx);
            var result = await repo.AddAsync(new PartyManagement.Domain.Entities.BankAccount
            {
                BankId = bankId,
                AccountNumber = accountNumber,
                AccountHolderName = holderName,
                AccountTypeId = accountTypeId,
                BranchId = branchId,
                IFSCCode = "HDFC0001234",
                IsDefaultAccount = false,
                IsPrimaryAccount = false,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            }, CancellationToken.None);
            return result.Id;
        }

        private async Task ClearTablesAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Party].[BankAccount]");
            await conn.ExecuteAsync("DELETE FROM [Party].[BankMaster]");
            await conn.ExecuteAsync("DELETE FROM [Party].[PartyGroup]");
            await conn.ExecuteAsync("DELETE FROM [Party].[MiscMaster]");
            await conn.ExecuteAsync("DELETE FROM [Party].[MiscTypeMaster]");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("BAQ_TYPE1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BAQ_M1");
            var bankId = await SeedBankMasterAsync("BAQ_BNK1", "Query Bank");
            var id = await SeedBankAccountAsync(bankId, miscId, miscId, "1111111111", "Jane Doe");

            var result = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.AccountNumber.Should().Be("1111111111");
            result.AccountHolderName.Should().Be("Jane Doe");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTablesAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("BAQ_TYPE2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BAQ_M2");
            var bankId = await SeedBankMasterAsync("BAQ_BNK2", "Soft Del Bank");
            var id = await SeedBankAccountAsync(bankId, miscId, miscId, "2222222222");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new BankAccountCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var result = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().BeNull();
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("BAQ_TYPE3");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BAQ_M3");
            var bankId = await SeedBankMasterAsync("BAQ_BNK3", "All Bank");
            await SeedBankAccountAsync(bankId, miscId, miscId, "3333333333");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, null, CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("BAQ_TYPE4");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BAQ_M4");
            var bankId = await SeedBankMasterAsync("BAQ_BNK4", "Del Bank");
            var id = await SeedBankAccountAsync(bankId, miscId, miscId, "4444444444");

            await using var ctx = _fixture.CreateFreshDbContext();
            await new BankAccountCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, null, CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_BankId()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("BAQ_TYPE5");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BAQ_M5");
            var bankId1 = await SeedBankMasterAsync("BAQ_BNK5A", "Filter Bank A");
            var bankId2 = await SeedBankMasterAsync("BAQ_BNK5B", "Filter Bank B");
            await SeedBankAccountAsync(bankId1, miscId, miscId, "5555555555");
            await SeedBankAccountAsync(bankId2, miscId, miscId, "6666666666");

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, bankId1, CancellationToken.None);

            items.Should().HaveCount(1);
            items[0].AccountNumber.Should().Be("5555555555");
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_Search()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("BAQ_TYPE6");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BAQ_M6");
            var bankId = await SeedBankMasterAsync("BAQ_BNK6", "Search Bank");
            await SeedBankAccountAsync(bankId, miscId, miscId, "7777777777", "Alpha Holder");
            await SeedBankAccountAsync(bankId, miscId, miscId, "8888888888", "Beta Holder");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "7777777777", null, CancellationToken.None);

            items.Should().HaveCount(1);
            items[0].AccountNumber.Should().Be("7777777777");
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Accounts()
        {
            await ClearTablesAsync();
            var miscTypeId = await SeedMiscTypeMasterAsync("BAQ_TYPE7");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BAQ_M7");
            var bankId = await SeedBankMasterAsync("BAQ_BNK7", "Autocomplete Bank");
            await SeedBankAccountAsync(bankId, miscId, miscId, "9090909090", "Auto Holder");

            var results = await CreateQueryRepo().AutocompleteAsync("9090909090", CancellationToken.None);

            results.Should().NotBeEmpty();
            results[0].AccountNumber.Should().Be("9090909090");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Empty_When_No_Match()
        {
            await ClearTablesAsync();

            var results = await CreateQueryRepo().AutocompleteAsync("DoesNotExist999", CancellationToken.None);

            results.Should().BeEmpty();
        }
    }
}
