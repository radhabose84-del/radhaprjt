using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using FinanceManagement.IntegrationTests.Common;
using FinanceManagement.Infrastructure.Repositories.CostCentre;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace FinanceManagement.IntegrationTests.Repositories.CostCentre
{
    [Collection("DatabaseCollection")]
    public sealed class CostCentreQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public CostCentreQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private CostCentreQueryRepository CreateQueryRepo()
        {
            var unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
            unitLookup.Setup(u => u.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int id, CancellationToken _) => new UnitLookupDto { UnitId = id, UnitName = "Test Unit" });

            var deptGroupLookup = new Mock<IDepartmentGroupLookup>(MockBehavior.Loose);
            var deptLookup = new Mock<IDepartmentLookup>(MockBehavior.Loose);
            deptLookup.Setup(d => d.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<DepartmentLookupDto>)new List<DepartmentLookupDto>());
            var userLookup = new Mock<IUserLookup>(MockBehavior.Loose);
            userLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UserLookupDto>)new List<UserLookupDto>());

            var conn = new SqlConnection(_fixture.ConnectionString);
            return new CostCentreQueryRepository(conn, unitLookup.Object, deptGroupLookup.Object, deptLookup.Object, userLookup.Object);
        }

        private async Task<int> SeedLevelAsync(int sortOrder = 1, string code = "CCL1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var type = await ctx.MiscTypeMaster.FirstOrDefaultAsync(t => t.MiscTypeCode == "COSTCENTRELEVEL");
            if (type == null)
            {
                type = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "COSTCENTRELEVEL",
                    Description = "Cost Centre Level",
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

        private async Task<int> SeedCostCentreAsync(int centreLevelId, string code = "STP", string name = "Plant", int unitId = 1, int? parentId = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var repo = new CostCentreCommandRepository(ctx);
            return await repo.CreateAsync(new Domain.Entities.CostCentre
            {
                UnitId = unitId,
                CompanyId = 1,
                CostCentreCode = code,
                CostCentreName = name,
                CentreLevelId = centreLevelId,
                ParentCostCentreId = parentId,
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
            await SeedCostCentreAsync(levelId);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Populate_UnitName()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await SeedCostCentreAsync(levelId);

            var (items, _) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1);

            items[0].UnitName.Should().Be("Test Unit");
            items[0].CentreLevelName.Should().Be("CCL1");
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            var id = await SeedCostCentreAsync(levelId);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new CostCentreCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, 1);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_Unit()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await SeedCostCentreAsync(levelId, "STP", "Plant", unitId: 1);

            var (items, total) = await CreateQueryRepo().GetAllAsync(1, 10, null, 2); // different unit

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Correct_Dto()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            var id = await SeedCostCentreAsync(levelId, "STP", "Plant");

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().NotBeNull();
            dto!.CostCentreCode.Should().Be("STP");
            dto.CostCentreName.Should().Be("Plant");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            var id = await SeedCostCentreAsync(levelId);
            await using (var ctx = _fixture.CreateFreshDbContext())
                await new CostCentreCommandRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id);

            dto.Should().BeNull();
        }

        // --- ALREADY EXISTS (per unit) ---

        [Fact]
        public async Task AlreadyExistsByCodeAsync_Should_Return_True_For_SameUnit()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await SeedCostCentreAsync(levelId, "STP", "Plant", unitId: 1);

            var exists = await CreateQueryRepo().AlreadyExistsByCodeAsync("STP", 1);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AlreadyExistsByCodeAsync_Should_Return_False_For_DifferentUnit()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await SeedCostCentreAsync(levelId, "STP", "Plant", unitId: 1);

            // Same code, different unit — allowed (per-unit uniqueness).
            var exists = await CreateQueryRepo().AlreadyExistsByCodeAsync("STP", 2);

            exists.Should().BeFalse();
        }

        // --- HIERARCHY HELPERS ---

        [Fact]
        public async Task GetCentreLevelSortOrderAsync_Should_Return_SortOrder()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync(sortOrder: 1, code: "CCL1");

            var sort = await CreateQueryRepo().GetCentreLevelSortOrderAsync(levelId);

            sort.Should().Be(1);
        }

        [Fact]
        public async Task ParentValidForLevelAsync_Should_Allow_Null_Parent_For_L1()
        {
            await ClearTableAsync();
            var l1Level = await SeedLevelAsync(sortOrder: 1, code: "CCL1");

            var valid = await CreateQueryRepo().ParentValidForLevelAsync(null, l1Level, 1);

            valid.Should().BeTrue();
        }

        [Fact]
        public async Task ParentValidForLevelAsync_Should_Require_L1_Parent_For_L2()
        {
            await ClearTableAsync();
            var l1Level = await SeedLevelAsync(sortOrder: 1, code: "CCL1");
            var l2Level = await SeedLevelAsync(sortOrder: 2, code: "CCL2");
            var l1Id = await SeedCostCentreAsync(l1Level, "STP", "Plant", unitId: 1);

            var valid = await CreateQueryRepo().ParentValidForLevelAsync(l1Id, l2Level, 1);

            valid.Should().BeTrue();
        }

        // --- DELETE GUARD ---

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_HasChildren()
        {
            await ClearTableAsync();
            var l1Level = await SeedLevelAsync(sortOrder: 1, code: "CCL1");
            var l2Level = await SeedLevelAsync(sortOrder: 2, code: "CCL2");
            var l1Id = await SeedCostCentreAsync(l1Level, "STP", "Plant", unitId: 1);
            await SeedCostCentreAsync(l2Level, "STPPROD", "Production", unitId: 1, parentId: l1Id);

            var linked = await CreateQueryRepo().SoftDeleteValidationAsync(l1Id);

            linked.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_NoChildren()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            var id = await SeedCostCentreAsync(levelId);

            var linked = await CreateQueryRepo().SoftDeleteValidationAsync(id);

            linked.Should().BeFalse();
        }

        [Fact]
        public async Task HasOpenTransactionsAsync_Should_Return_False_Stub()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            var id = await SeedCostCentreAsync(levelId);

            var hasOpen = await CreateQueryRepo().HasOpenTransactionsAsync(id);

            hasOpen.Should().BeFalse();
        }

        // --- AUTOCOMPLETE ---

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Records()
        {
            await ClearTableAsync();
            var levelId = await SeedLevelAsync();
            await SeedCostCentreAsync(levelId, "STP", "Plant");

            var results = await CreateQueryRepo().AutocompleteAsync("STP", 1, null, CancellationToken.None);

            results.Should().ContainSingle();
            results[0].CostCentreCode.Should().Be("STP");
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Filter_By_Level()
        {
            await ClearTableAsync();
            var l1Level = await SeedLevelAsync(sortOrder: 1, code: "CCL1");
            var l2Level = await SeedLevelAsync(sortOrder: 2, code: "CCL2");
            var l1Id = await SeedCostCentreAsync(l1Level, "STP", "Plant", unitId: 1);
            await SeedCostCentreAsync(l2Level, "STPPROD", "Production", unitId: 1, parentId: l1Id);

            var results = await CreateQueryRepo().AutocompleteAsync(string.Empty, 1, l1Level, CancellationToken.None);

            results.Should().ContainSingle();
            results[0].CostCentreCode.Should().Be("STP");
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
