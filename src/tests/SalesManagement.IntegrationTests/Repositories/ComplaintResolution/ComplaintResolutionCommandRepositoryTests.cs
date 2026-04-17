using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.ComplaintResolution;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.ComplaintResolution
{
    /// <summary>
    /// Integration tests for ComplaintResolutionCommandRepository.
    /// ComplaintResolution depends on ComplaintHeader and MiscMaster (ResolutionType).
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ComplaintResolutionCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ComplaintResolutionCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ComplaintResolutionCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new ComplaintResolutionCommandRepository(ctx);

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx, string code = "CRC_MT")
        {
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Resolution Cmd Test",
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(mt);
                await ctx.SaveChangesAsync();
            }
            return mt.Id;
        }

        private async Task<int> EnsureMiscAsync(ApplicationDbContext ctx, int miscTypeId, string code)
        {
            var m = await ctx.MiscMaster.FirstOrDefaultAsync(x => x.MiscTypeId == miscTypeId && x.Code == code);
            if (m == null)
            {
                m = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscTypeId,
                    Code = code,
                    Description = code,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(m);
                await ctx.SaveChangesAsync();
            }
            return m.Id;
        }

        /// <summary>
        /// Seeds: MiscType -> statuses -> ComplaintHeader.
        /// Returns (complaintId, resolutionTypeId).
        /// </summary>
        private async Task<(int complaintId, int resTypeId)> SeedPrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CRC_STATUS");
            var resTypeId = await EnsureMiscAsync(ctx, mtId, "CRC_RESTYPE");

            var complaint = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = "CRC_C" + Guid.NewGuid().ToString("N")[..6],
                ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId = 100,
                StatusId = statusId,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintHeader.AddAsync(complaint);
            await ctx.SaveChangesAsync();

            return (complaint.Id, resTypeId);
        }

        private SalesManagement.Domain.Entities.ComplaintResolution BuildEntity(
            int complaintHeaderId, int resolutionTypeId)
            => new SalesManagement.Domain.Entities.ComplaintResolution
            {
                ComplaintHeaderId = complaintHeaderId,
                ResolutionTypeId = resolutionTypeId,
                ResolutionSummary = "Test resolution summary",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync() => await _fixture.ClearTablesAsync(
            "Sales.ComplaintDepartmentFeedback",
            "Sales.ComplaintFeedbackAttachment",
            "Sales.ComplaintQCReviewAssignment",
            "Sales.ComplaintResolution",
            "Sales.ComplaintQCReview",
            "Sales.ComplaintDetailNature",
            "Sales.ComplaintDetail",
            "Sales.ComplaintAttachment",
            "Sales.ComplaintHeader");

        // ---------------------------------------------------------------------------
        // CreateAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await ClearAsync();
            var (complaintId, resTypeId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(complaintId, resTypeId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearAsync();
            var (complaintId, resTypeId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity(complaintId, resTypeId);
            entity.ResolutionSummary = "Detailed resolution";
            entity.CreditAmount = 1000m;
            entity.ReturnQuantity = 5m;

            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintResolution.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ComplaintHeaderId.Should().Be(complaintId);
            saved.ResolutionTypeId.Should().Be(resTypeId);
            saved.ResolutionSummary.Should().Be("Detailed resolution");
            saved.CreditAmount.Should().Be(1000m);
            saved.ReturnQuantity.Should().Be(5m);
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            var (complaintId, resTypeId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(complaintId, resTypeId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintResolution.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // UpdateAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task UpdateAsync_Should_Return_UpdatedId()
        {
            await ClearAsync();
            var (complaintId, resTypeId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(complaintId, resTypeId));
            ctx.ChangeTracker.Clear();

            var updateEntity = new SalesManagement.Domain.Entities.ComplaintResolution
            {
                Id = id,
                ResolutionTypeId = resTypeId,
                ResolutionSummary = "Updated",
                IsActive = Status.Active
            };

            var resultId = await CreateRepository(ctx).UpdateAsync(updateEntity);

            resultId.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await ClearAsync();
            var (complaintId, resTypeId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(complaintId, resTypeId));
            ctx.ChangeTracker.Clear();

            var updateEntity = new SalesManagement.Domain.Entities.ComplaintResolution
            {
                Id = id,
                ResolutionTypeId = resTypeId,
                ResolutionSummary = "Updated summary",
                CreditAmount = 2000m,
                ClosureRemarks = "Closing now",
                IsActive = Status.Inactive
            };

            await CreateRepository(ctx).UpdateAsync(updateEntity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintResolution.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ResolutionSummary.Should().Be("Updated summary");
            saved.CreditAmount.Should().Be(2000m);
            saved.ClosureRemarks.Should().Be("Closing now");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateAsync(
                new SalesManagement.Domain.Entities.ComplaintResolution
                {
                    Id = 99999,
                    ResolutionTypeId = 1,
                    IsActive = Status.Active
                });

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Set_ClosedBy_When_Provided()
        {
            await ClearAsync();
            var (complaintId, resTypeId) = await SeedPrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(complaintId, resTypeId));
            ctx.ChangeTracker.Clear();

            var now = DateTimeOffset.UtcNow;
            var updateEntity = new SalesManagement.Domain.Entities.ComplaintResolution
            {
                Id = id,
                ResolutionTypeId = resTypeId,
                ResolutionSummary = "Closed",
                ClosedBy = 42,
                ClosedDate = now,
                IsActive = Status.Active
            };

            await CreateRepository(ctx).UpdateAsync(updateEntity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintResolution.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ClosedBy.Should().Be(42);
            saved.ClosedDate.Should().NotBeNull();
        }
    }
}
