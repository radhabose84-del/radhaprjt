using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.Complaint;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.Complaint
{
    /// <summary>
    /// Integration tests for ComplaintCommandRepository.
    /// ComplaintHeader has a complex CreateAsync(entity, typeId) that increments Finance.DocumentSequence.
    /// Tests validate Create, SoftDelete, and attachment operations.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class ComplaintCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public ComplaintCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private ComplaintCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new ComplaintCommandRepository(ctx);

        // ---------------------------------------------------------------------------
        // Seeding helpers
        // ---------------------------------------------------------------------------

        private async Task<int> EnsureMiscTypeAsync(ApplicationDbContext ctx, string code = "CCR_MT")
        {
            var mt = await ctx.MiscTypeMaster.FirstOrDefaultAsync(x => x.MiscTypeCode == code);
            if (mt == null)
            {
                mt = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = code,
                    Description = "Complaint Cmd Test Type",
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

        private SalesManagement.Domain.Entities.ComplaintHeader BuildEntity(
            int? statusId = null,
            string complaintNumber = "CCR_C001",
            int customerId = 100)
            => new SalesManagement.Domain.Entities.ComplaintHeader
            {
                ComplaintNumber = complaintNumber,
                ComplaintDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                CustomerId = customerId,
                StatusId = statusId,
                Remarks = "Test complaint",
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
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_STATUS");

            var entity = BuildEntity(statusId, "CCR_C001");
            // typeId = 1 → first Finance.DocumentSequence row (seeded by DbFixture)
            var newId = await CreateRepository(ctx).CreateAsync(entity, 1);

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_PERSIST");

            var entity = BuildEntity(statusId, "CCR_PERST", customerId: 42);
            entity.CreditLimit = 50000;
            entity.Remarks = "Persist test";

            var newId = await CreateRepository(ctx).CreateAsync(entity, 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ComplaintNumber.Should().Be("CCR_PERST");
            saved.CustomerId.Should().Be(42);
            saved.CreditLimit.Should().Be(50000);
            saved.Remarks.Should().Be("Persist test");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_AUDIT");

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_AUD01"), 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ---------------------------------------------------------------------------
        // SoftDeleteAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_EntityExists()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_SD1");

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_SD01"), 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_SD2");

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_SD02"), 1);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.ComplaintHeader
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_When_AlreadyDeleted()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_SD3");

            var id = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_SD03"), 1);
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }

        // ---------------------------------------------------------------------------
        // AddAttachmentAsync / DeleteAttachmentAsync
        // ---------------------------------------------------------------------------

        [Fact]
        public async Task AddAttachmentAsync_Should_Return_NewId()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_ATT1");
            var complaintId = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_ATT01"), 1);
            ctx.ChangeTracker.Clear();

            var attachment = new SalesManagement.Domain.Entities.ComplaintAttachment
            {
                ComplaintHeaderId = complaintId,
                FileName = "test.pdf",
                FilePath = "/uploads/test.pdf",
                FileType = "application/pdf",
                FileSize = 1024,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var attId = await CreateRepository(ctx).AddAttachmentAsync(attachment);

            attId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task DeleteAttachmentAsync_Should_Return_True_When_Exists()
        {
            await ClearAsync();
            await using var ctx = _fixture.CreateFreshDbContext();
            var mtId = await EnsureMiscTypeAsync(ctx);
            var statusId = await EnsureMiscAsync(ctx, mtId, "CCR_ATT2");
            var complaintId = await CreateRepository(ctx).CreateAsync(BuildEntity(statusId, "CCR_ATT02"), 1);
            ctx.ChangeTracker.Clear();

            var attachment = new SalesManagement.Domain.Entities.ComplaintAttachment
            {
                ComplaintHeaderId = complaintId,
                FileName = "del.pdf",
                FilePath = "/uploads/del.pdf",
                FileType = "application/pdf",
                FileSize = 512,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
            var attId = await CreateRepository(ctx).AddAttachmentAsync(attachment);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).DeleteAttachmentAsync(attId, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAttachmentAsync_Should_Return_False_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).DeleteAttachmentAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
