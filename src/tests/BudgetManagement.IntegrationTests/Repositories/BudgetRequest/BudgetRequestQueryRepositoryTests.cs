using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Infrastructure.Repositories.BudgetRequest;
using BudgetManagement.IntegrationTests.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;

namespace BudgetManagement.IntegrationTests.Repositories.BudgetRequest
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetRequestQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        public BudgetRequestQueryRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;

            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockMiscRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.MiscMaster?)null);

            _mockUnitLookup
                .Setup(l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitLookupDto { UnitId = 1, UnitName = "TestUnit", ShortName = "TU" });
        }

        private BudgetRequestCommandRepository CreateCommandRepo(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _mockMiscRepo.Object, _mockUnitLookup.Object, _fixture.IpMock.Object);

        private BudgetRequestQueryRepository CreateQueryRepo() =>
            new(new SqlConnection(_fixture.ConnectionString),
                NullLogger<BudgetRequestQueryRepository>.Instance,
                _fixture.IpMock.Object,
                _mockMiscRepo.Object);

        private static BudgetManagement.Domain.Entities.BudgetRequest BuildEntity(
            int unitId = 1,
            int financialYearId = 1,
            int statusId = 1) =>
            new()
            {
                RequestCode = "TU-2025-01",
                UnitId = unitId,
                CurrencyId = 1,
                RequestTypeId = 1,
                RequestAmount = 10000m,
                FinancialYearId = financialYearId,
                StatusId = statusId,
                BudgetGroupId = 1,
                RequestById = 1,
                FromDate = DateOnly.FromDateTime(DateTime.Today),
                ToDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task<int> SeedEntityAsync(int unitId = 1, int financialYearId = 1)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity(unitId: unitId, financialYearId: financialYearId);
            var result = await CreateCommandRepo(ctx).AddAsync(entity, CancellationToken.None);
            return result.Id;
        }

        private async Task ClearTableAsync()
        {
            await _fixture.ClearAllTablesAsync();
            await _fixture.SeedPrerequisiteDataAsync();
        }

        // --- GET ALL ---

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded_Record()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, total) = await CreateQueryRepo().GetAllAsync(
                statusId: null, pageNumber: 1, pageSize: 10, searchTerm: null, CancellationToken.None);

            items.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var (items, total) = await CreateQueryRepo().GetAllAsync(
                statusId: null, pageNumber: 1, pageSize: 10, searchTerm: null, CancellationToken.None);

            items.Should().BeEmpty();
            total.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearTableAsync();
            await SeedEntityAsync();

            var (items, _) = await CreateQueryRepo().GetAllAsync(
                statusId: null, pageNumber: 1, pageSize: 10, searchTerm: "NONEXISTENT_TERM_XYZ", CancellationToken.None);

            items.Should().BeEmpty();
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Dto_When_Found()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            dto.Should().NotBeNull();
            dto!.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearTableAsync();
            var id = await SeedEntityAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            await CreateCommandRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            var dto = await CreateQueryRepo().GetByIdAsync(id, CancellationToken.None);

            dto.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await ClearTableAsync();

            var dto = await CreateQueryRepo().GetByIdAsync(9999, CancellationToken.None);

            dto.Should().BeNull();
        }

        // --- ALLOCATION EXISTS ---

        [Fact]
        public async Task AllocationExistsAsync_Should_Return_False_For_New_Entry()
        {
            await ClearTableAsync();

            var exists = await CreateQueryRepo().AllocationExistsAsync(
                financialYearId: 99, requestById: 99,
                requestMonthId: 1, budgetGroupId: 1,
                projectId: null, wbsId: null,
                CancellationToken.None);

            exists.Should().BeFalse();
        }
    }
}

