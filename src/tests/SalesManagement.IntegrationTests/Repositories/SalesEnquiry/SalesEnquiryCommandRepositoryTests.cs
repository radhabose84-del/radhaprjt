using Contracts.Interfaces.Lookups.Finance;
using Microsoft.EntityFrameworkCore;
using Moq;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesEnquiry;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesEnquiry
{
    [Collection("DatabaseCollection")]
    public sealed class SalesEnquiryCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeqLookup;

        public SalesEnquiryCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
            _mockDocSeqLookup = new Mock<IDocumentSequenceLookup>(MockBehavior.Loose);
            _mockDocSeqLookup
                .Setup(d => d.IncrementDocNoAsync(It.IsAny<int>(), It.IsAny<System.Data.Common.DbConnection>(), It.IsAny<System.Data.Common.DbTransaction>()))
                .Returns(Task.CompletedTask);
        }

        private SalesEnquiryCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx, _mockDocSeqLookup.Object);

        // Idempotently seeds Sales.MiscTypeMaster (ENQ_TYPE) + Sales.MiscMaster (ENQ_DOMESTIC).
        // Returns the MiscMaster.Id for use in EnquiryTypeId.
        private static async Task<int> EnsureEnqDomesticAsync(ApplicationDbContext ctx)
        {
            var miscType = await ctx.MiscTypeMaster
                .FirstOrDefaultAsync(t => t.MiscTypeCode == "ENQ_TYPE" && t.IsDeleted == IsDelete.NotDeleted);
            if (miscType == null)
            {
                miscType = new SalesManagement.Domain.Entities.MiscTypeMaster
                {
                    MiscTypeCode = "ENQ_TYPE", Description = "Enquiry Type",
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscTypeMaster.AddAsync(miscType);
                await ctx.SaveChangesAsync();
            }

            var misc = await ctx.MiscMaster
                .FirstOrDefaultAsync(m => m.MiscTypeId == miscType.Id && m.Code == "ENQ_DOMESTIC" && m.IsDeleted == IsDelete.NotDeleted);
            if (misc == null)
            {
                misc = new SalesManagement.Domain.Entities.MiscMaster
                {
                    MiscTypeId = miscType.Id, Code = "ENQ_DOMESTIC", Description = "Domestic", SortOrder = 1,
                    IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
                };
                await ctx.MiscMaster.AddAsync(misc);
                await ctx.SaveChangesAsync();
            }
            return misc.Id;
        }

        private SalesManagement.Domain.Entities.SalesEnquiryHeader BuildEntity(
            int partyId = 100, string? contactPerson = "John", int detailCount = 2, int enquiryTypeId = 1)
        {
            var entity = new SalesManagement.Domain.Entities.SalesEnquiryHeader
            {
                PartyId = partyId,
                EnquiryDate = DateTimeOffset.UtcNow,
                EnquiryTypeId = enquiryTypeId,
                ContactPerson = contactPerson,
                ExpectedDeliveryDate = DateTimeOffset.UtcNow.AddDays(14),
                PaymentTermId = null,
                Remarks = "test enquiry",
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted,
                SalesEnquiryDetails = Enumerable.Range(1, detailCount).Select(i =>
                    new SalesManagement.Domain.Entities.SalesEnquiryDetail
                    {
                        ItemId = i * 10,
                        Quantity = i * 5m,
                        ExmillRate = 100m,
                        TargetPrice = 95m,
                        Discount = 5m
                    }).ToList()
            };
            return entity;
        }

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var enqId = await EnsureEnqDomesticAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(partyId: 100, enquiryTypeId: enqId), 1);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var enqId = await EnsureEnqDomesticAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(partyId: 200, contactPerson: "Alice", detailCount: 3, enquiryTypeId: enqId), 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesEnquiryHeader.FirstAsync(x => x.Id == id);
            saved.PartyId.Should().Be(200);
            saved.ContactPerson.Should().Be("Alice");
            saved.EnquiryTypeId.Should().Be(enqId);

            var details = await ctx.SalesEnquiryDetail.Where(d => d.SalesEnquiryHeaderId == id).ToListAsync();
            details.Should().HaveCount(3);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Header_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var enqId = await EnsureEnqDomesticAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(partyId: 300, contactPerson: "Bob", enquiryTypeId: enqId), 1);
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(partyId: 999, contactPerson: "NewName", enquiryTypeId: enqId);
            updated.Id = id;
            updated.Remarks = "updated";
            updated.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.SalesEnquiryHeader.FirstAsync(x => x.Id == id);
            reloaded.PartyId.Should().Be(999);
            reloaded.ContactPerson.Should().Be("NewName");
            reloaded.Remarks.Should().Be("updated");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_EnquiryTypeId_Change()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var enqDomesticId = await EnsureEnqDomesticAsync(ctx);

            // Seed second MiscMaster row (ENQ_EXPORT) under the same parent type
            var miscType = await ctx.MiscTypeMaster.FirstAsync(t => t.MiscTypeCode == "ENQ_TYPE");
            var enqExport = new SalesManagement.Domain.Entities.MiscMaster
            {
                MiscTypeId = miscType.Id, Code = "ENQ_EXPORT", Description = "Export", SortOrder = 2,
                IsActive = Status.Active, IsDeleted = IsDelete.NotDeleted
            };
            await ctx.MiscMaster.AddAsync(enqExport);
            await ctx.SaveChangesAsync();
            ctx.ChangeTracker.Clear();

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(enquiryTypeId: enqDomesticId), 1);
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(enquiryTypeId: enqExport.Id);
            updated.Id = id;
            await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.SalesEnquiryHeader.FirstAsync(x => x.Id == id);
            reloaded.EnquiryTypeId.Should().Be(enqExport.Id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var enqId = await EnsureEnqDomesticAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(detailCount: 2, enquiryTypeId: enqId), 1);
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(detailCount: 4, enquiryTypeId: enqId);
            updated.Id = id;

            await CreateRepo(ctx).UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var details = await ctx.SalesEnquiryDetail.Where(d => d.SalesEnquiryHeaderId == id).ToListAsync();
            details.Should().HaveCount(4);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var enqId = await EnsureEnqDomesticAsync(ctx);

            var ghost = BuildEntity(enquiryTypeId: enqId);
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var enqId = await EnsureEnqDomesticAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(enquiryTypeId: enqId), 1);
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.SalesEnquiryHeader.IgnoreQueryFilters().FirstAsync(x => x.Id == id);
            reloaded.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var enqId = await EnsureEnqDomesticAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(enquiryTypeId: enqId), 1);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
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
