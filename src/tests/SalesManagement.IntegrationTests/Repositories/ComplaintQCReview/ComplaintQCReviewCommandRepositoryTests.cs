using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.ComplaintQCReview;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.ComplaintQCReview
{
    /// <summary>
    /// Integration tests for ComplaintQCReviewCommandRepository.
    /// ComplaintQCReview depends on ComplaintHeader, so prerequisites are seeded first.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ComplaintQCReviewCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ComplaintQCReviewCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ComplaintQCReviewCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new ComplaintQCReviewCommandRepository(ctx);

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx, string code = "QCRC_MT")
        {
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "QCReview Cmd Test Type",
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

        private async Task<(int complaintId, int pvId)> EnsurePrerequisitesAsync()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var pvId = await EnsureMiscAsync(ctx, mtId, "QCRC_PV");
            var statusId = await EnsureMiscAsync(ctx, mtId, "QCRC_STATUS");

            var complaint = new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = "QCRC_C" + Guid.NewGuid().ToString("N")[..6],
                ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId = 100,
                StatusId = statusId,
                Remarks = "QCReview prerequisite",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            await ctx.ComplaintHeader.AddAsync(complaint);
            await ctx.SaveChangesAsync();

            return (complaint.Id, pvId);
        }

        private SalesManagement.Domain.Entities.ComplaintQCReview BuildEntity(
            int complaintHeaderId, int pvId)
            => new SalesManagement.Domain.Entities.ComplaintQCReview
            {
                ComplaintHeaderId = complaintHeaderId,
                PhysicalVerificationId = pvId,
                LabVerificationRequired = false,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync() =>
            await _fixture.ClearTablesAsync(
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
            var (complaintId, pvId) = await EnsurePrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(complaintId, pvId));

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearAsync();
            var (complaintId, pvId) = await EnsurePrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var entity = BuildEntity(complaintId, pvId);
            entity.LabVerificationRequired = true;
            entity.Comments = "Test QC review";

            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintQCReview.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ComplaintHeaderId.Should().Be(complaintId);
            saved.PhysicalVerificationId.Should().Be(pvId);
            saved.LabVerificationRequired.Should().BeTrue();
            saved.Comments.Should().Be("Test QC review");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            var (complaintId, pvId) = await EnsurePrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(complaintId, pvId));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintQCReview.FirstOrDefaultAsync(x => x.Id == newId);

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
            var (complaintId, pvId) = await EnsurePrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(complaintId, pvId));
            ctx.ChangeTracker.Clear();

            var updateEntity = new SalesManagement.Domain.Entities.ComplaintQCReview
            {
                Id = id,
                PhysicalVerificationId = pvId,
                LabVerificationRequired = true,
                Comments = "Updated",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var resultId = await CreateRepository(ctx).UpdateAsync(
                updateEntity,
                new List<SalesManagement.Domain.Entities.ComplaintQCReviewAssignment>());

            resultId.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await ClearAsync();
            var (complaintId, pvId) = await EnsurePrerequisitesAsync();

            await using var ctx = _fixture.CreateFreshDbContext();
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(complaintId, pvId));
            ctx.ChangeTracker.Clear();

            var updateEntity = new SalesManagement.Domain.Entities.ComplaintQCReview
            {
                Id = id,
                PhysicalVerificationId = pvId,
                LabVerificationRequired = true,
                Comments = "Updated comments",
                IsActive = Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

            await CreateRepository(ctx).UpdateAsync(
                updateEntity,
                new List<SalesManagement.Domain.Entities.ComplaintQCReviewAssignment>());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintQCReview.FirstOrDefaultAsync(x => x.Id == id);
            saved!.LabVerificationRequired.Should().BeTrue();
            saved.Comments.Should().Be("Updated comments");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateAsync(
                new SalesManagement.Domain.Entities.ComplaintQCReview
                {
                    Id = 99999,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                },
                new List<SalesManagement.Domain.Entities.ComplaintQCReviewAssignment>());

            result.Should().Be(0);
        }
    }
}
