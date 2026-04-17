using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Validations.SalesManagement;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductionManagement.Infrastructure.Data;
using ProductionManagement.Infrastructure.Repositories.LotMaster;
using ProductionManagement.IntegrationTests.Common;
using static ProductionManagement.Domain.Common.BaseEntity;

namespace ProductionManagement.IntegrationTests.Repositories.LotMaster
{
    [Collection("DatabaseCollection")]
    public sealed class LotMasterQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public LotMasterQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private LotMasterQueryRepository CreateRepo(
            Mock<IItemLookup>? itemLookup = null,
            Mock<IUnitLookup>? unitLookup = null,
            Mock<ILotMasterSalesValidation>? salesValidation = null)
        {
            if (itemLookup == null)
            {
                itemLookup = new Mock<IItemLookup>(MockBehavior.Loose);
                itemLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto>());
            }
            if (unitLookup == null)
            {
                unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
                unitLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<UnitLookupDto>)new List<UnitLookupDto>());
            }
            if (salesValidation == null)
            {
                salesValidation = new Mock<ILotMasterSalesValidation>(MockBehavior.Loose);
                salesValidation.Setup(s => s.HasLinkedLotMasterAsync(It.IsAny<int>())).ReturnsAsync(false);
                salesValidation.Setup(s => s.HasActiveLotMasterAsync(It.IsAny<int>())).ReturnsAsync(false);
            }

            return new LotMasterQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                itemLookup.Object, unitLookup.Object, salesValidation.Object);
        }

        private async Task<(int LotTypeId, int StatusId)> EnsureMiscAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var t = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "LMQ_TYP");
            if (t == null)
            {
                t = new Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "LMQ_TYP", Description = "type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(t);
                await ctx.SaveChangesAsync();
            }
            var lotType = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "LMQ_LT");
            if (lotType == null)
            {
                lotType = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "LMQ_LT", Description = "LT",
                    SortOrder = 1, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(lotType);
                await ctx.SaveChangesAsync();
            }
            var status = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.Code == "LMQ_ST");
            if (status == null)
            {
                status = new Domain.Entities.MiscMaster
                {
                    MiscTypeId = t.Id, Code = "LMQ_ST", Description = "ST",
                    SortOrder = 2, IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(status);
                await ctx.SaveChangesAsync();
            }
            return (lotType.Id, status.Id);
        }

        private async Task<int> SeedAsync(string code, Status active = Status.Active, IsDelete deleted = IsDelete.NotDeleted)
        {
            var (lotTypeId, statusId) = await EnsureMiscAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var lm = new Domain.Entities.LotMaster
            {
                LotCode = code, BatchNumber = code + "B",
                LotTypeId = lotTypeId, StatusId = statusId,
                ItemId = 1, UnitId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                TotalProducedQty = 100m, AvailableQty = 100m,
                IsActive = active, IsDeleted = deleted
            };
            await ctx.LotMaster.AddAsync(lm);
            await ctx.SaveChangesAsync();
            return lm.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("LMQ1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("LMQDEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Matching()
        {
            await ClearAsync();
            var id = await SeedAsync("LMQID");

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.LotCode.Should().Be("LMQID");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_SoftDeleted()
        {
            await ClearAsync();
            var id = await SeedAsync("LMQSD", deleted: IsDelete.Deleted);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("LMAC1");
            await SeedAsync("LMAC2", active: Status.Inactive);

            var result = await CreateRepo().AutocompleteAsync("LMAC", null, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task AlreadyExistsAsync_Should_Return_True_For_Duplicate()
        {
            await ClearAsync();
            await SeedAsync("LMDUP");

            var result = await CreateRepo().AlreadyExistsAsync("LMDUP");

            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task LotTypeExistsAsync_Should_Return_True_For_Existing_Misc()
        {
            var (lotTypeId, _) = await EnsureMiscAsync();

            var result = await CreateRepo().LotTypeExistsAsync(lotTypeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task StatusExistsAsync_Should_Return_True_For_Existing_Misc()
        {
            var (_, statusId) = await EnsureMiscAsync();

            var result = await CreateRepo().StatusExistsAsync(statusId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ItemExistsAsync_Should_Return_True_When_Lookup_Returns_Match()
        {
            var itemLookup = new Mock<IItemLookup>(MockBehavior.Loose);
            itemLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto> { new() { Id = 1 } });

            var result = await CreateRepo(itemLookup: itemLookup).ItemExistsAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task ItemExistsAsync_Should_Return_False_When_Lookup_Returns_Empty()
        {
            var result = await CreateRepo().ItemExistsAsync(9999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task UnitExistsAsync_Should_Return_True_When_Lookup_Returns_Match()
        {
            var unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
            unitLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UnitLookupDto>)new List<UnitLookupDto> { new() { UnitId = 1, UnitName = "U1" } });

            var result = await CreateRepo(unitLookup: unitLookup).UnitExistsAsync(1);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("LMSDV1");

            var result = await CreateRepo().SoftDeleteValidationAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteValidationAsync_Should_Return_True_When_SalesValidation_Links()
        {
            await ClearAsync();
            var id = await SeedAsync("LMSDV2");
            var sales = new Mock<ILotMasterSalesValidation>(MockBehavior.Loose);
            sales.Setup(s => s.HasLinkedLotMasterAsync(id)).ReturnsAsync(true);

            var result = await CreateRepo(salesValidation: sales).SoftDeleteValidationAsync(id);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task IsLotMasterLinkedAsync_Should_Return_False_When_No_Refs()
        {
            await ClearAsync();
            var id = await SeedAsync("LMLK1");

            var result = await CreateRepo().IsLotMasterLinkedAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task VariantBelongsToItemAsync_Should_Return_True_When_Parent_Matches()
        {
            var itemLookup = new Mock<IItemLookup>(MockBehavior.Loose);
            itemLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto> { new() { Id = 5, ParentItemId = 99 } });

            var result = await CreateRepo(itemLookup: itemLookup).VariantBelongsToItemAsync(5, 99);

            result.Should().BeTrue();
        }
    }
}
