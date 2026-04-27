using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesReturn;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesReturn
{
    [Collection("DatabaseCollection")]
    public sealed class SalesReturnCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public SalesReturnCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private SalesReturnCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

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

        private async Task<(int complaintStatusId, int returnStatusId, int complaintId)> EnsureComplaintAsync(string complaintNumber = "SRC_CMP1")
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == "SRC_MT");
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "SRC_MT", Description = "Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            var complaintStatusId = await EnsureMiscAsync(ctx, mt.Id, "SRC_CS");
            var returnStatusId = await EnsureMiscAsync(ctx, mt.Id, "SRC_RS");

            var complaint = await ctx.ComplaintHeader.FirstOrDefaultAsync(x => x.ComplaintNumber == complaintNumber);
            if (complaint == null)
            {
                complaint = new SalesManagement.Domain.Entities.ComplaintHeader
                {
                    ComplaintNumber = complaintNumber,
                    ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                    CustomerId = 100,
                    StatusId = complaintStatusId,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.ComplaintHeader.AddAsync(complaint);
                await ctx.SaveChangesAsync();
            }
            return (complaintStatusId, returnStatusId, complaint.Id);
        }

        private async Task<SalesManagement.Domain.Entities.SalesReturnHeader> BuildHeaderAsync(string returnNumber = "SR001")
        {
            var (_, returnStatusId, complaintId) = await EnsureComplaintAsync();
            return new SalesManagement.Domain.Entities.SalesReturnHeader
            {
                ReturnNumber = returnNumber,
                ReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                ComplaintHeaderId = complaintId,
                CustomerId = 100,
                WarehouseId = 1,
                BinId = 1,
                StatusId = returnStatusId,
                Remarks = "test return",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
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
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync();

            var entity = await BuildHeaderAsync("SR_C1");
            var id = await CreateRepo(ctx).CreateAsync(entity, typeId: 99);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync();

            var entity = await BuildHeaderAsync("SR_C2");
            var id = await CreateRepo(ctx).CreateAsync(entity, typeId: 99);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesReturnHeader.FirstAsync(x => x.Id == id);
            saved.ReturnNumber.Should().Be("SR_C2");
            saved.CustomerId.Should().Be(100);
        }

        [Fact]
        public async Task CreateAsync_Should_Increment_DocumentSequence()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync(typeId: 98);

            // Reset seq to 0 and create
            await using var cnn = new SqlConnection(_fixture.ConnectionString);
            await cnn.OpenAsync();
            await cnn.ExecuteAsync("UPDATE [Finance].[DocumentSequence] SET DocNo = 0 WHERE TransactionTypeId = 98");

            var entity = await BuildHeaderAsync("SR_C3");
            await CreateRepo(ctx).CreateAsync(entity, typeId: 98);

            var docNo = await cnn.ExecuteScalarAsync<int>(
                "SELECT DocNo FROM [Finance].[DocumentSequence] WHERE TransactionTypeId = 98");
            docNo.Should().Be(1);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            await EnsureDocumentSequenceAsync();
            var id = await CreateRepo(ctx).CreateAsync(await BuildHeaderAsync("SR_D1"), 99);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            result.Should().BeTrue();
            var reloaded = await ctx.SalesReturnHeader.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepo(ctx).SoftDeleteAsync(9999999, CancellationToken.None);

            result.Should().BeFalse();
        }

        // -----------------------------------------------------------------------
        // Auto-close: UpdateComplaintResolutionReturnStatusAsync — closure branch
        // -----------------------------------------------------------------------

        private async Task<int> SeedComplaintResolutionAsync(int complaintHeaderId, int? existingClosureStatusId = null)
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mt = await ctx.MiscTypeMaster.FirstAsync(x => x.MiscTypeCode == "SRC_MT");
            var defaultMiscId = await EnsureMiscAsync(ctx, mt.Id, "SRC_DEF");

            var resolution = new SalesManagement.Domain.Entities.ComplaintResolution
            {
                ComplaintHeaderId = complaintHeaderId,
                ResolutionTypeId = defaultMiscId,
                ResolutionSummary = "seeded for auto-close test",
                ClosureStatusId = existingClosureStatusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintResolution.AddAsync(resolution);
            await ctx.SaveChangesAsync();
            return resolution.Id;
        }

        [Fact]
        public async Task UpdateComplaintResolutionReturnStatusAsync_WithClosure_SetsClosedFields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var (_, returnStatusId, complaintId) = await EnsureComplaintAsync("SRC_AC1");
            var resolutionId = await SeedComplaintResolutionAsync(complaintId);

            var mt = await ctx.MiscTypeMaster.FirstAsync(x => x.MiscTypeCode == "SRC_MT");
            var closedStatusId = await EnsureMiscAsync(ctx, mt.Id, "SRC_CLOSED");

            await CreateRepo(ctx).UpdateComplaintResolutionReturnStatusAsync(
                complaintId, returnStatusId, returnQuantity: 8m,
                closureStatusId: closedStatusId, closedBy: 42);

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var saved = await verifyCtx.ComplaintResolution.FirstAsync(x => x.Id == resolutionId);
            saved.ReturnStatusId.Should().Be(returnStatusId);
            saved.ReturnQuantity.Should().Be(8m);
            saved.ClosureStatusId.Should().Be(closedStatusId);
            saved.ClosedBy.Should().Be(42);
            saved.ClosedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateComplaintResolutionReturnStatusAsync_WithoutClosure_LeavesClosedFieldsUntouched()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var (_, returnStatusId, complaintId) = await EnsureComplaintAsync("SRC_AC2");
            var resolutionId = await SeedComplaintResolutionAsync(complaintId, existingClosureStatusId: null);

            await CreateRepo(ctx).UpdateComplaintResolutionReturnStatusAsync(
                complaintId, returnStatusId, returnQuantity: 3m,
                closureStatusId: null, closedBy: null);

            await using var verifyCtx = _fixture.CreateFreshDbContext();
            var saved = await verifyCtx.ComplaintResolution.FirstAsync(x => x.Id == resolutionId);
            saved.ReturnStatusId.Should().Be(returnStatusId);
            saved.ReturnQuantity.Should().Be(3m);
            saved.ClosureStatusId.Should().BeNull();
            saved.ClosedBy.Should().BeNull();
            saved.ClosedDate.Should().BeNull();
        }

        [Fact]
        public async Task UpdateComplaintResolutionReturnStatusAsync_NoMatchingResolution_DoesNothing()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var (_, returnStatusId, _) = await EnsureComplaintAsync("SRC_AC3");

            // No resolution seeded for header id 999999 — should silently no-op
            var act = async () => await CreateRepo(ctx).UpdateComplaintResolutionReturnStatusAsync(
                complaintHeaderId: 999999, returnStatusId: returnStatusId, returnQuantity: 1m,
                closureStatusId: 1, closedBy: 1);

            await act.Should().NotThrowAsync();
        }
    }
}
