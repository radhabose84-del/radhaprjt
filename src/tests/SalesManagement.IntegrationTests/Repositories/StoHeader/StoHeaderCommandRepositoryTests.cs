using Contracts.Interfaces.Lookups.Finance;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.StoHeader;
using SalesManagement.IntegrationTests.Common;
using System.Data;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.StoHeader
{
    [Collection("DatabaseCollection")]
    public sealed class StoHeaderCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public StoHeaderCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private StoHeaderCommandRepository CreateRepo(ApplicationDbContext ctx, Mock<IDocumentSequenceLookup>? docSeq = null)
        {
            if (docSeq == null)
            {
                docSeq = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
                docSeq.Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<IDbConnection>(), It.IsAny<IDbTransaction>()))
                    .Returns(Task.CompletedTask);
            }
            return new StoHeaderCommandRepository(ctx, docSeq.Object);
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

        private async Task<(int stoTypeId, int movementId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            // StoLineItemStatus type with Draft code (needed by GetDraftLineStatusIdAsync)
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
            await EnsureMiscAsync(ctx, lineType.Id, "Draft");

            // StoApprovalStatus = "ApprovalStatus" with Pending (needed by GetDraftHeaderStatusIdAsync)
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
            await EnsureMiscAsync(ctx, approvalType.Id, "Pending");
            await EnsureMiscAsync(ctx, approvalType.Id, "Approved");
            await EnsureMiscAsync(ctx, approvalType.Id, "Rejected");

            // Auxiliary misc for MovementTypeConfig FKs
            var aux = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SHC_AUX");
            if (aux == null)
            {
                aux = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SHC_AUX", Description = "Aux",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(aux);
                await ctx.SaveChangesAsync();
            }
            var cat = await EnsureMiscAsync(ctx, aux.Id, "SHC_CAT");
            var fromType = await EnsureMiscAsync(ctx, aux.Id, "SHC_FROM");
            var toType = await EnsureMiscAsync(ctx, aux.Id, "SHC_TO");

            var movement = await ctx.MovementTypeConfig.FirstOrDefaultAsync(x => x.MovementCode == "SHCM");
            if (movement == null)
            {
                movement = new SalesManagement.Domain.Entities.MovementTypeConfig
                {
                    MovementCode = "SHCM", MovementDescription = "STO",
                    MovementCategoryId = cat,
                    FromStockTypeId = fromType,
                    ToStockTypeId = toType,
                    QuantityUpdateFlag = true,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MovementTypeConfig.AddAsync(movement);
                await ctx.SaveChangesAsync();
            }

            var stoType = await ctx.StoTypeMaster.FirstOrDefaultAsync(x => x.StoTypeCode == "SHCT");
            if (stoType == null)
            {
                stoType = new SalesManagement.Domain.Entities.StoTypeMaster
                {
                    StoTypeCode = "SHCT", StoTypeName = "STO Type",
                    PgiMovementTypeId = movement.Id,
                    GrMovementTypeId = movement.Id,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.StoTypeMaster.AddAsync(stoType);
                await ctx.SaveChangesAsync();
            }

            return (stoType.Id, movement.Id);
        }

        private async Task<SalesManagement.Domain.Entities.StoHeader> BuildEntityAsync(
            string stoNumber = "STO001", int detailCount = 2)
        {
            var (stoTypeId, movementId) = await EnsurePrerequisitesAsync();
            return new SalesManagement.Domain.Entities.StoHeader
            {
                StoNumber = stoNumber,
                DocumentDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                ExpectedDeliveryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(5)),
                StoTypeId = stoTypeId,
                MovementTypeId = movementId,
                SupplyingPlantId = 1, SupplyingStorageLocationId = 1,
                ReceivingPlantId = 2, ReceivingStorageLocationId = 2,
                Remarks = "test STO",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                StoDetails = Enumerable.Range(1, detailCount).Select(i =>
                    new SalesManagement.Domain.Entities.StoDetail
                    {
                        ItemId = i * 10,
                        Quantity = i * 100m,
                        UOMId = 1,
                        TransferPrice = 50m
                    }).ToList()
            };
        }

        private async Task EnsureDocumentSequenceAsync(int typeId = 99)
        {
            await using var cnn = new SqlConnection(_fixture.ConnectionString);
            await cnn.OpenAsync();
            await cnn.ExecuteAsync(
                "IF NOT EXISTS (SELECT 1 FROM [Finance].[DocumentSequence] WHERE TransactionTypeId = @TypeId) INSERT INTO [Finance].[DocumentSequence] (TransactionTypeId, FinancialYearId, DocNo) VALUES (@TypeId, 1, 0)",
                new { TypeId = typeId });
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearTablesAsync(
                "Sales.StoDetail", "Sales.StoHeader");

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STO_C1"), typeId: 99);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_HeaderStatus_To_Pending()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STO_C2"), typeId: 99);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.StoHeader.FirstAsync(x => x.Id == id);
            saved.HeaderStatusId.Should().NotBeNull();
            var status = await ctx.MiscMaster.FirstAsync(m => m.Id == saved.HeaderStatusId);
            status.Code.Should().Be("Pending");
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Details_With_Draft_LineStatus()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync();

            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STO_C3", detailCount: 3), typeId: 99);
            ctx.ChangeTracker.Clear();

            var details = await ctx.StoDetail.Where(d => d.StoHeaderId == id).ToListAsync();
            details.Should().HaveCount(3);
            details.Should().AllSatisfy(d => d.LineStatusId.Should().NotBeNull());
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Header_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STO_U1"), 99);
            ctx.ChangeTracker.Clear();

            var updated = await BuildEntityAsync("STO_U1");
            updated.Id = id;
            updated.Remarks = "updated";
            updated.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.StoHeader.FirstAsync(x => x.Id == id);
            reloaded.Remarks.Should().Be("updated");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Not_Change_StoNumber()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STO_ORIG"), 99);
            ctx.ChangeTracker.Clear();

            var updated = await BuildEntityAsync("STO_DIFFERENT");
            updated.Id = id;

            await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.StoHeader.FirstAsync(x => x.Id == id);
            reloaded.StoNumber.Should().Be("STO_ORIG"); // immutable
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = await BuildEntityAsync("GHOST");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateApprovalStatusAsync_Should_Set_Status()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STO_AP1"), 99);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).UpdateApprovalStatusAsync(id, "Approved", 7, "tester", "127.0.0.1", CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.StoHeader.FirstAsync(x => x.Id == id);
            var status = await ctx.MiscMaster.FirstAsync(m => m.Id == reloaded.HeaderStatusId);
            status.Code.Should().Be("Approved");
        }

        [Fact]
        public async Task UpdateApprovalStatusAsync_Should_Be_Noop_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await EnsurePrerequisitesAsync();

            // Should not throw
            await CreateRepo(ctx).UpdateApprovalStatusAsync(9999999, "Approved", 7, "tester", "127.0.0.1", CancellationToken.None);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync();
            var id = await CreateRepo(ctx).CreateAsync(await BuildEntityAsync("STO_D1"), 99);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.StoHeader.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
