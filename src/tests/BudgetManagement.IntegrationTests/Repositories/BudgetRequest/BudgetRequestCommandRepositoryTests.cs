using BudgetManagement.Application.Common.Interfaces.IMiscMaster;
using BudgetManagement.Infrastructure.Repositories.BudgetRequest;
using BudgetManagement.IntegrationTests.Common;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.EntityFrameworkCore;

namespace BudgetManagement.IntegrationTests.Repositories.BudgetRequest
{
    [Collection("DatabaseCollection")]
    public sealed class BudgetRequestCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);

        public BudgetRequestCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;

            // MiscMaster mock: GetMiscMasterByName returns a placeholder (for AddAsync pending-status)
            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new BudgetManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockMiscRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((BudgetManagement.Domain.Entities.MiscMaster?)null);

            // UnitLookup mock for GenerateCodeAsync
            _mockUnitLookup
                .Setup(l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitLookupDto { UnitId = 1, UnitName = "TestUnit", ShortName = "TU" });
        }

        private BudgetRequestCommandRepository CreateRepository(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            new(ctx, _mockMiscRepo.Object, _mockUnitLookup.Object, _fixture.IpMock.Object);

        private static BudgetManagement.Domain.Entities.BudgetRequest BuildEntity(
            int unitId = 1,
            int financialYearId = 1,
            decimal requestAmount = 10000m) =>
            new()
            {
                RequestCode = "TU-2025-01",
                UnitId = unitId,
                CurrencyId = 1,
                RequestTypeId = 1,
                RequestAmount = requestAmount,
                FinancialYearId = financialYearId,
                StatusId = 1,
                BudgetGroupId = 1,
                RequestById = 1,
                FromDate = DateOnly.FromDateTime(DateTime.Today),
                ToDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(BudgetManagement.Infrastructure.Data.ApplicationDbContext ctx) =>
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Budget.BudgetRequest");

        // --- ADD ---

        [Fact]
        public async Task AddAsync_Should_Return_Entity_WithId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity();
            var result = await CreateRepository(ctx).AddAsync(entity, CancellationToken.None);

            result.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task AddAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity(unitId: 1, requestAmount: 25000m);
            var result = await CreateRepository(ctx).AddAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BudgetRequests.FirstOrDefaultAsync(x => x.Id == result.Id);

            saved!.UnitId.Should().Be(1);
            saved.RequestAmount.Should().Be(25000m);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        // --- SOFT DELETE ---

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var entity = BuildEntity();
            var added = await CreateRepository(ctx).AddAsync(entity, CancellationToken.None);

            var result = await CreateRepository(ctx).SoftDeleteAsync(added.Id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var entity = BuildEntity();
            var added = await CreateRepository(ctx).AddAsync(entity, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(added.Id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.BudgetRequests.FirstOrDefaultAsync(x => x.Id == added.Id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(9999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // --- GET BY ID ---

        [Fact]
        public async Task GetByIdAsync_Should_Return_Entity_When_Found()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var entity = BuildEntity();
            var added = await CreateRepository(ctx).AddAsync(entity, CancellationToken.None);

            var result = await CreateRepository(ctx).GetByIdAsync(added.Id, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(added.Id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).GetByIdAsync(9999, CancellationToken.None);

            result.Should().BeNull();
        }

        // --- EXISTS ---

        [Fact]
        public async Task ExistsOpexAsync_Should_Return_True_When_Duplicate()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var entity = BuildEntity(unitId: 1, financialYearId: 1);
            entity.BudgetGroupId = 5;
            entity.ProjectId = null;
            await CreateRepository(ctx).AddAsync(entity, CancellationToken.None);

            var exists = await CreateRepository(ctx).ExistsOpexAsync(
                unitId: 1, financialYearId: 1, requestTypeId: 1,
                budgetGroupId: 5, requestById: 1, ct: CancellationToken.None);

            exists.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsOpexAsync_Should_Return_False_When_No_Match()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var exists = await CreateRepository(ctx).ExistsOpexAsync(
                unitId: 99, financialYearId: 99, requestTypeId: 99,
                budgetGroupId: 99, requestById: null, ct: CancellationToken.None);

            exists.Should().BeFalse();
        }
    }
}

