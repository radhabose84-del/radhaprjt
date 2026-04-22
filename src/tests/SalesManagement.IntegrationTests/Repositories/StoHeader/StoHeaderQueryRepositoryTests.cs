using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Users;
using Contracts.Dtos.Lookups.Warehouse;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.StoHeader;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.StoHeader
{
    [Collection("DatabaseCollection")]
    public sealed class StoHeaderQueryRepositoryTests
    {
        private readonly DbFixture _fixture;
        public StoHeaderQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private StoHeaderQueryRepository CreateRepo(
            Mock<IUnitLookup>? unitLookup = null,
            Mock<IWarehouseLookup>? warehouseLookup = null,
            Mock<IItemLookup>? itemLookup = null,
            Mock<IUOMLookup>? uomLookup = null,
            Mock<IUserLookup>? userLookup = null,
            Mock<IIPAddressService>? ip = null,
            Mock<IDataAccessFilter>? dataAccessFilter = null)
        {
            if (unitLookup == null)
            {
                unitLookup = new Mock<IUnitLookup>(MockBehavior.Loose);
                unitLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<UnitLookupDto>)ids.Select(id =>
                            new UnitLookupDto { UnitId = id, UnitName = "Plant " + id }).ToList());
            }
            if (warehouseLookup == null)
            {
                warehouseLookup = new Mock<IWarehouseLookup>(MockBehavior.Loose);
                warehouseLookup.Setup(w => w.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<WarehouseLookupDto>)ids.Select(id =>
                            new WarehouseLookupDto { Id = id, WarehouseName = "WH " + id }).ToList());
            }
            if (itemLookup == null)
            {
                itemLookup = new Mock<IItemLookup>(MockBehavior.Loose);
                itemLookup.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<ItemLookupDto>)ids.Select(id =>
                            new ItemLookupDto { Id = id, ItemCode = "I" + id, ItemName = "Item " + id }).ToList());
            }
            if (uomLookup == null)
            {
                uomLookup = new Mock<IUOMLookup>(MockBehavior.Loose);
                uomLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IEnumerable<int> ids, CancellationToken _) =>
                        (IReadOnlyList<UOMLookupDto>)ids.Select(id =>
                            new UOMLookupDto { Id = id, UOMName = "KG" }).ToList());
            }
            if (userLookup == null)
            {
                userLookup = new Mock<IUserLookup>(MockBehavior.Loose);
                userLookup.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((IReadOnlyList<UserLookupDto>)new List<UserLookupDto>());
            }
            if (ip == null)
            {
                ip = new Mock<IIPAddressService>(MockBehavior.Loose);
                ip.Setup(x => x.GetUserId()).Returns(1);
            }
            if (dataAccessFilter == null)
            {
                dataAccessFilter = new Mock<IDataAccessFilter>(MockBehavior.Loose);
                dataAccessFilter
                    .Setup(f => f.GetContextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new DataAccessContext { BypassDataAccess = true });
            }

            return new StoHeaderQueryRepository(
                new SqlConnection(_fixture.ConnectionString),
                unitLookup.Object, warehouseLookup.Object, itemLookup.Object,
                uomLookup.Object, userLookup.Object, ip.Object,
                dataAccessFilter.Object);
        }

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId, Code = code, Description = code,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        private async Task<(int stoTypeId, int movementId, int pendingStatusId, int approvedStatusId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var lineType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "StoLineItemStatus");
            if (lineType == null)
            {
                lineType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "StoLineItemStatus", Description = "StoLineItemStatus",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(lineType);
                await ctx.SaveChangesAsync();
            }
            var draftLineId = await EnsureMiscAsync(ctx, lineType.Id, "Draft");

            var approvalType = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "ApprovalStatus");
            if (approvalType == null)
            {
                approvalType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ApprovalStatus", Description = "ApprovalStatus",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(approvalType);
                await ctx.SaveChangesAsync();
            }
            else if (approvalType.Description != "ApprovalStatus")
            {
                approvalType.Description = "ApprovalStatus";
                await ctx.SaveChangesAsync();
            }
            var pendingId = await EnsureMiscAsync(ctx, approvalType.Id, "Pending");
            var approvedId = await EnsureMiscAsync(ctx, approvalType.Id, "Approved");

            var aux = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SHQ_AUX");
            if (aux == null)
            {
                aux = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SHQ_AUX", Description = "Aux",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(aux);
                await ctx.SaveChangesAsync();
            }
            var cat = await EnsureMiscAsync(ctx, aux.Id, "SHQ_CAT");
            var fromType = await EnsureMiscAsync(ctx, aux.Id, "SHQ_FROM");
            var toType = await EnsureMiscAsync(ctx, aux.Id, "SHQ_TO");

            var movement = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.MovementCode == "SHQM");
            if (movement == null)
            {
                movement = new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "SHQM", MovementDescription = "STO",
                    MovementCategoryId = cat,
                    FromStockTypeId = fromType,
                    ToStockTypeId = toType,
                    QuantityUpdateFlag = true,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MovementTypeConfig.AddAsync(movement);
                await ctx.SaveChangesAsync();
            }

            var stoType = await ctx.StoTypeMaster.FirstOrDefaultAsync(x => x.StoTypeCode == "SHQT");
            if (stoType == null)
            {
                stoType = new SalesManagement.Domain.Entities.StoTypeMaster
                {
                    StoTypeCode = "SHQT", StoTypeName = "STO Type",
                    PgiMovementTypeId = movement.Id,
                    GrMovementTypeId = movement.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.StoTypeMaster.AddAsync(stoType);
                await ctx.SaveChangesAsync();
            }

            return (stoType.Id, movement.Id, pendingId, approvedId);
        }

        private async Task<int> SeedAsync(string stoNumber = "STO_Q1", int detailCount = 2,
            IsDelete deleted = IsDelete.NotDeleted, Status active = Status.Active, int? statusId = null,
            bool approved = false)
        {
            var (stoTypeId, movementId, pendingId, approvedId) = await EnsurePrerequisitesAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var h = new SalesManagement.Domain.Entities.StoHeader
            {
                StoNumber = stoNumber,
                DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5)),
                StoTypeId = stoTypeId,
                MovementTypeId = movementId,
                SupplyingPlantId = 1, SupplyingStorageLocationId = 1,
                ReceivingPlantId = 2, ReceivingStorageLocationId = 2,
                Remarks = "test",
                HeaderStatusId = statusId ?? (approved ? approvedId : pendingId),
                IsActive = active, IsDeleted = deleted,
                StoDetails = Enumerable.Range(1, detailCount).Select(i =>
                    new SalesManagement.Domain.Entities.StoDetail
                    {
                        ItemId = i * 10, Quantity = i * 100m, UOMId = 1, TransferPrice = 50m
                    }).ToList()
            };
            await ctx.StoHeader.AddAsync(h);
            await ctx.SaveChangesAsync();
            return h.Id;
        }

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync(
                "Sales.StoDetail", "Sales.StoHeader");

        [Fact]
        public async Task GetAllAsync_Should_Return_Seeded()
        {
            await ClearAsync();
            await SeedAsync("STO_A1");

            var (rows, total) = await CreateRepo().GetAllAsync(1, 10, null);

            rows.Should().HaveCount(1);
            total.Should().Be(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Filter_By_SearchTerm()
        {
            await ClearAsync();
            await SeedAsync("UNIQUE_STO_Z");
            await SeedAsync("OTHER_STO");

            var (rows, _) = await CreateRepo().GetAllAsync(1, 10, "UNIQUE_STO_Z");

            rows.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetAllAsync_Should_Exclude_SoftDeleted()
        {
            await ClearAsync();
            await SeedAsync("STO_DEL", deleted: IsDelete.Deleted);

            var (_, total) = await CreateRepo().GetAllAsync(1, 10, null);

            total.Should().Be(0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_With_Details()
        {
            await ClearAsync();
            var id = await SeedAsync("STO_B1", detailCount: 3);

            var result = await CreateRepo().GetByIdAsync(id);

            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await CreateRepo().GetByIdAsync(9999999);
            result.Should().BeNull();
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Return_Active_Matching()
        {
            await ClearAsync();
            await SeedAsync("STO_AC", approved: true);

            var result = await CreateRepo().AutocompleteAsync("STO_AC", CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task AutocompleteAsync_Should_Exclude_Inactive()
        {
            await ClearAsync();
            await SeedAsync("STO_INACT", active: Status.Inactive, approved: true);

            var result = await CreateRepo().AutocompleteAsync("STO_INACT", CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_True_For_Missing()
        {
            var result = await CreateRepo().NotFoundAsync(9999999);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task NotFoundAsync_Should_Return_False_When_Exists()
        {
            await ClearAsync();
            var id = await SeedAsync("STO_NF");

            var result = await CreateRepo().NotFoundAsync(id);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task StoTypeExistsAsync_Should_Return_True_For_Active()
        {
            var (stoTypeId, _, _, _) = await EnsurePrerequisitesAsync();

            var result = await CreateRepo().StoTypeExistsAsync(stoTypeId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task StoTypeExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateRepo().StoTypeExistsAsync(9999999);
            result.Should().BeFalse();
        }

        [Fact]
        public async Task MovementTypeExistsAsync_Should_Return_True_For_Active()
        {
            var (_, movementId, _, _) = await EnsurePrerequisitesAsync();

            var result = await CreateRepo().MovementTypeExistsAsync(movementId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task MovementTypeExistsAsync_Should_Return_False_When_Missing()
        {
            var result = await CreateRepo().MovementTypeExistsAsync(9999999);
            result.Should().BeFalse();
        }

        // Note: GetPendingAsync requires cross-module AppData.MiscMaster schema (Workflow module)
        // which is not provisioned in this test fixture. Coverage deferred.
    }
}
