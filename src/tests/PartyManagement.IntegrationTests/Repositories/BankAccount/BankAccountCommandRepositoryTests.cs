using Microsoft.EntityFrameworkCore;
using PartyManagement.Domain.Common;
using PartyManagement.Infrastructure.Repositories.BankAccount;
using PartyManagement.Infrastructure.Repositories.BankMaster;
using PartyManagement.Infrastructure.Repositories.MiscMaster;
using PartyManagement.Infrastructure.Repositories.MiscTypeMaster;

namespace PartyManagement.IntegrationTests.Repositories.BankAccount
{
    [Collection("DatabaseCollection")]
    public sealed class BankAccountCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BankAccountCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private BankAccountCommandRepository CreateRepository(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx);

        private async Task<int> SeedMiscTypeMasterAsync(string code = "BA_MISC_TYPE")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new MiscTypeMasterCommandRepository(ctx);
            var result = await repo.CreateAsync(new PartyManagement.Domain.Entities.MiscTypeMaster
            {
                MiscTypeCode = code,
                Description = "Test BankAccount Misc Type",
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            });
            return result.Id;
        }

        private async Task<int> SeedMiscMasterAsync(int miscTypeId, string code = "BA_MM001")
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

        private async Task<int> SeedBankMasterAsync(string code = "TST001", string name = "Test Bank")
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

        private static PartyManagement.Domain.Entities.BankAccount BuildEntity(
            int bankId,
            int accountTypeId,
            int branchId,
            string accountNumber = "1234567890",
            string holderName = "Test Holder") =>
            new PartyManagement.Domain.Entities.BankAccount
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
            };

        private async Task ClearTablesAsync(PartyManagement.Infrastructure.Data.ApplicationDbContext ctx) => await _fixture.ClearAllTablesAsync();

        // --- ADD (CREATE) ---

        [Fact]
        public async Task AddAsync_Should_Return_Entity_With_Id_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("BA_TYPE_C1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BA_C1");
            var bankId = await SeedBankMasterAsync("BANK_C1", "Create Test Bank");

            var result = await CreateRepository(ctx).AddAsync(BuildEntity(bankId, miscId, miscId), CancellationToken.None);

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("BA_TYPE_C2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BA_C2");
            var bankId = await SeedBankMasterAsync("BANK_C2", "Fields Bank");

            var result = await CreateRepository(ctx).AddAsync(
                BuildEntity(bankId, miscId, miscId, "9876543210", "John Doe"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BankAccount.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved.Should().NotBeNull();
            saved!.AccountNumber.Should().Be("9876543210");
            saved.AccountHolderName.Should().Be("John Doe");
            saved.BankId.Should().Be(bankId);
            saved.AccountTypeId.Should().Be(miscId);
            saved.BranchId.Should().Be(miscId);
            saved.IsActive.Should().Be(BaseEntity.Status.Active);
            saved.IsDeleted.Should().Be(BaseEntity.IsDelete.NotDeleted);
        }

        [Fact]
        public async Task AddAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("BA_TYPE_C3");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BA_C3");
            var bankId = await SeedBankMasterAsync("BANK_C3", "Audit Bank");

            var result = await CreateRepository(ctx).AddAsync(BuildEntity(bankId, miscId, miscId), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BankAccount.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedDate.Should().NotBeNull();
        }

        // --- UPDATE ---

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("BA_TYPE_U1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BA_U1");
            var bankId = await SeedBankMasterAsync("BANK_U1", "Update Bank");

            var entity = await CreateRepository(ctx).AddAsync(
                BuildEntity(bankId, miscId, miscId, "1111111111", "Original Holder"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BankAccount.FirstAsync(x => x.Id == entity.Id);
            saved.AccountHolderName = "Updated Holder";
            await CreateRepository(ctx).UpdateAsync(saved, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.BankAccount.FirstAsync(x => x.Id == entity.Id);
            updated.AccountHolderName.Should().Be("Updated Holder");
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_AccountNumber()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("BA_TYPE_U2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BA_U2");
            var bankId = await SeedBankMasterAsync("BANK_U2", "Immutable Bank");

            var entity = await CreateRepository(ctx).AddAsync(
                BuildEntity(bankId, miscId, miscId, "9999999999", "Holder"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BankAccount.FirstAsync(x => x.Id == entity.Id);
            saved.AccountHolderName = "Different Holder";
            await CreateRepository(ctx).UpdateAsync(saved, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var updated = await ctx.BankAccount.FirstAsync(x => x.Id == entity.Id);
            updated.AccountNumber.Should().Be("9999999999");
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("BA_TYPE_D1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BA_D1");
            var bankId = await SeedBankMasterAsync("BANK_D1", "Delete Bank");

            var entity = await CreateRepository(ctx).AddAsync(
                BuildEntity(bankId, miscId, miscId, "3333333333"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(entity.Id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("BA_TYPE_D2");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BA_D2");
            var bankId = await SeedBankMasterAsync("BANK_D2", "Soft Del Bank");

            var entity = await CreateRepository(ctx).AddAsync(
                BuildEntity(bankId, miscId, miscId, "4444444444"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(entity.Id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var deleted = await ctx.BankAccount
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == entity.Id);

            deleted!.IsDeleted.Should().Be(BaseEntity.IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // --- FIND ---

        [Fact]
        public async Task FindAsync_Should_Return_Entity_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);
            var miscTypeId = await SeedMiscTypeMasterAsync("BA_TYPE_F1");
            var miscId = await SeedMiscMasterAsync(miscTypeId, "BA_F1");
            var bankId = await SeedBankMasterAsync("BANK_F1", "Find Bank");

            var entity = await CreateRepository(ctx).AddAsync(
                BuildEntity(bankId, miscId, miscId, "5555555555"), CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var found = await CreateRepository(ctx).FindAsync(entity.Id, CancellationToken.None);

            found.Should().NotBeNull();
            found!.AccountNumber.Should().Be("5555555555");
        }

        [Fact]
        public async Task FindAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTablesAsync(ctx);

            var found = await CreateRepository(ctx).FindAsync(9999, CancellationToken.None);

            found.Should().BeNull();
        }
    }
}
