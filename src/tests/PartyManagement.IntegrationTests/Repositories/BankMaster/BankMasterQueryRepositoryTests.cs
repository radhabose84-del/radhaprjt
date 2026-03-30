using Dapper;
using Microsoft.Data.SqlClient;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.BankMaster;

namespace PartyManagement.IntegrationTests.Repositories.BankMaster
{
    [Collection("DatabaseCollection")]
    public sealed class BankMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BankMasterQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BankMasterQueryRepository CreateQueryRepo()
        {
            var conn = new SqlConnection(_fixture.ConnectionString);
            return new BankMasterQueryRepository(conn);
        }

        private async Task<int> SeedEntityAsync(string code = "BNK_QRY001", string name = "Query Test Bank")
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

        private async Task ClearTableAsync()
        {
            await using var conn = new SqlConnection(_fixture.ConnectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync("DELETE FROM [Party].[BankAccount]");
            await conn.ExecuteAsync("DELETE FROM [Party].[BankMaster]");
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("BNK_DEL1", "Delete Me");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.BankMaster.FindAsync(id);
            entity!.IsDeleted = BaseEntity.IsDelete.Deleted;
            await new BankMasterCommandRepository(ctx).SoftDeleteAsync(entity, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_Search()
        {
            await ClearTableAsync();
            await SeedEntityAsync("BNK_A1", "Alpha Bank");
            await SeedEntityAsync("BNK_B1", "Beta Bank");

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, "Alpha", CancellationToken.None);

            items.Should().HaveCount(1);
            items[0].BankName.Should().Be("Alpha Bank");
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Entity()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("BNK_ID1", "Get By Id Bank");

            var result = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.BankName.Should().Be("Get By Id Bank");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var result = await CreateQueryRepo().GetByIdAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("BNK_DEL2", "Soft Deleted Bank");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.BankMaster.FindAsync(id);
            entity!.IsDeleted = BaseEntity.IsDelete.Deleted;
            await new BankMasterCommandRepository(ctx).SoftDeleteAsync(entity, CancellationToken.None);

            var result = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            result.Should().BeNull();
        }

        // --- EXISTS BY BANK CODE ---

        [Fact]
        public async Task ExistsByBankCodeAsync_Should_Return_True_When_Duplicate()
        {
            await ClearTableAsync();
            await SeedEntityAsync("BNK_EX1", "Existing Bank");

            // ExistsByBankCodeAsync checks BankName (not BankCode) in its SQL — pass the bank name
            var exists = await CreateQueryRepo().ExistsByBankCodeAsync("Existing Bank", null, CancellationToken.None);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsByBankCodeAsync_Should_Return_False_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync("BNK_EX2", "To Delete Bank");

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = await ctx.BankMaster.FindAsync(id);
            entity!.IsDeleted = BaseEntity.IsDelete.Deleted;
            await new BankMasterCommandRepository(ctx).SoftDeleteAsync(entity, CancellationToken.None);

            // ExistsByBankCodeAsync checks BankName (not BankCode) in its SQL — pass the bank name
            var exists = await CreateQueryRepo().ExistsByBankCodeAsync("To Delete Bank", null, CancellationToken.None);

            exists.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task GetAutocompleteAsync_Should_Return_Active_Banks()
        {
            await ClearTableAsync();
            await SeedEntityAsync("BNK_AC1", "Autocomplete Bank");

            var results = await CreateQueryRepo().GetAutocompleteAsync("Autocomplete", CancellationToken.None);

            results.Should().NotBeEmpty();
            results[0].BankName.Should().Be("Autocomplete Bank");
        }
    }
}
