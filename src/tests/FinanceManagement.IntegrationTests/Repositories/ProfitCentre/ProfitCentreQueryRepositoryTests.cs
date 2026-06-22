using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.ProfitCentre;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.ProfitCentre
{
    [Collection("DatabaseCollection")]
    public sealed class ProfitCentreQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ProfitCentreQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private ProfitCentreQueryRepository CreateQueryRepo()
        {
            var companyLookup = new Mock<ICompanyLookup>(MockBehavior.Loose);
            companyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new CompanyLookupDto { CompanyId = 1, CompanyName = "Test Company" }
                });

            var userLookup = new Mock<IUserLookup>(MockBehavior.Loose);
            userLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UserLookupDto>)new List<UserLookupDto>());

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new ProfitCentreQueryRepository(conn, companyLookup.Object, userLookup.Object);
        }

        private async Task<int> SeedLevelAsync(int sortOrder = 1, string code = "PCL1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "PROFITCENTRELEVEL");
            if (type == null)
            {
                type = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "PROFITCENTRELEVEL",
                    Description = "Profit Centre Level",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                ctx.MiscTypeMaster.Add(type);
                await ctx.SaveChangesAsync();
            }

            var misc = new Domain.Entities.MiscMaster
            {
                MiscTypeId = type.Id,
                Code = code,
                Description = code,
                SortOrder = sortOrder,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            ctx.MiscMaster.Add(misc);
            await ctx.SaveChangesAsync();
            return misc.Id;
        }

        private async Task<int> SeedProfitCentreAsync(int levelId, string code = "PCSPIN", string name = "Spinning", int? parentId = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new ProfitCentreCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.ProfitCentre
            {
                CompanyId = 1,
                ProfitCentreCode = code,
                ProfitCentreName = name,
                LevelId = levelId,
                ParentProfitCentreId = parentId,
                IsRevenueLinked = true,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            });
        }

        private async Task ClearTableAsync() => await _fixture.ClearAllTablesAsync();

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await SeedProfitCentreAsync(levelId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_CompanyName_And_LevelName()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await SeedProfitCentreAsync(levelId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items[0].CompanyName.Should().Be("Test Company");
            items[0].LevelName.Should().Be("PCL1");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            var id = await SeedProfitCentreAsync(levelId);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new ProfitCentreCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            var id = await SeedProfitCentreAsync(levelId, "PCSPIN", "Spinning");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.ProfitCentreCode.Should().Be("PCSPIN");
            dto.ProfitCentreName.Should().Be("Spinning");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            var id = await SeedProfitCentreAsync(levelId);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new ProfitCentreCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- ALREADY EXISTS (global, across companies) ---

        [Fact]
        public async Task AlreadyExistsByCodeAsync_Should_Return_True_For_DuplicateCode()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await SeedProfitCentreAsync(levelId, "PCSPIN", "Spinning");

            var exists = await CreateQueryRepo().AlreadyExistsByCodeAsync("PCSPIN");

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsByCodeAsync_Should_Return_False_For_NewCode()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await SeedProfitCentreAsync(levelId, "PCSPIN", "Spinning");

            var exists = await CreateQueryRepo().AlreadyExistsByCodeAsync("PCWEAV");

            exists.Should().BeFalse();
        }

        // --- HIERARCHY HELPERS ---

        [Fact]
        public async Task GetLevelSortOrderAsync_Should_Return_SortOrder()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync(sortOrder: 1, code: "PCL1");

            var sort = await CreateQueryRepo().GetLevelSortOrderAsync(levelId);

            sort.Should().Be(1);
        }

        [Fact]
        public async Task ParentValidForLevelAsync_Should_Allow_Null_Parent_For_L1()
        {
            await ClearTableAsync();
            var l1Level = await SeedLevelAsync(sortOrder: 1, code: "PCL1");

            var valid = await CreateQueryRepo().ParentValidForLevelAsync(null, l1Level);

            valid.Should().BeTrue();
        }

        [Fact]
        public async Task ParentValidForLevelAsync_Should_Require_L1_Parent_For_L2()
        {
            await ClearTableAsync();
            var l1Level = await SeedLevelAsync(sortOrder: 1, code: "PCL1");
            var l2Level = await SeedLevelAsync(sortOrder: 2, code: "PCL2");
            var l1Id = await SeedProfitCentreAsync(l1Level, "PCSPIN", "Spinning");

            var valid = await CreateQueryRepo().ParentValidForLevelAsync(l1Id, l2Level);

            valid.Should().BeTrue();
        }

        // --- DELETE GUARD ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_HasChildren()
        {
            await ClearTableAsync();
            var l1Level = await SeedLevelAsync(sortOrder: 1, code: "PCL1");
            var l2Level = await SeedLevelAsync(sortOrder: 2, code: "PCL2");
            var l1Id = await SeedProfitCentreAsync(l1Level, "PCSPIN", "Spinning");
            await SeedProfitCentreAsync(l2Level, "PCSPIN001", "Cotton Spinning", parentId: l1Id);

            var linked = await CreateQueryRepo().SoftDeleteValidationAsync(l1Id);

            linked.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_NoChildren()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            var id = await SeedProfitCentreAsync(levelId);

            var linked = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            linked.Should().BeFalse();
        }

        [Fact]
        public async Task HasCurrentYearTransactionsAsync_Should_Return_False_Stub()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            var id = await SeedProfitCentreAsync(levelId);

            var hasTxns = await CreateQueryRepo().HasCurrentYearTransactionsAsync(id);

            hasTxns.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Records()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await SeedProfitCentreAsync(levelId, "PCSPIN", "Spinning");

            var results = await CreateQueryRepo().AutocompleteAsync("PCSPIN", null, CancellationToken.None);

            results.Should().ContainSingle();
            results[0].ProfitCentreCode.Should().Be("PCSPIN");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Filter_By_Level()
        {
            await ClearTableAsync();
            var l1Level = await SeedLevelAsync(sortOrder: 1, code: "PCL1");
            var l2Level = await SeedLevelAsync(sortOrder: 2, code: "PCL2");
            var l1Id = await SeedProfitCentreAsync(l1Level, "PCSPIN", "Spinning");
            await SeedProfitCentreAsync(l2Level, "PCSPIN001", "Cotton Spinning", parentId: l1Id);

            var results = await CreateQueryRepo().AutocompleteAsync(string.Empty, l1Level, CancellationToken.None);

            results.Should().ContainSingle();
            results[0].ProfitCentreCode.Should().Be("PCSPIN");
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_When_Missing()
        {
            await ClearTableAsync();

            var notFound = await CreateQueryRepo().NotFoundAsync(9999);

            notFound.Should().BeTrue();
        }
    }
}
