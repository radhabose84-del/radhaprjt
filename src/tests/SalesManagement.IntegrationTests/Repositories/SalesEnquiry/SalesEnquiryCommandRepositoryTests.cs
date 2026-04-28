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

        private SalesManagement.Domain.Entities.SalesEnquiryHeader BuildEntity(
            int partyId = 100, string? contactPerson = "John", int detailCount = 2)
        {
            var entity = new SalesManagement.Domain.Entities.SalesEnquiryHeader
            {
                PartyId = partyId,
                EnquiryDate = DateTimeOffset.UtcNow,
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

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(partyId: 100), 1);

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_And_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(partyId: 200, contactPerson: "Alice", detailCount: 3), 1);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesEnquiryHeader.FirstAsync(x => x.Id == id);
            saved.PartyId.Should().Be(200);
            saved.ContactPerson.Should().Be("Alice");

            var details = await ctx.SalesEnquiryDetail.Where(d => d.SalesEnquiryHeaderId == id).ToListAsync();
            details.Should().HaveCount(3);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Header_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(partyId: 300, contactPerson: "Bob"), 1);
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(partyId: 999, contactPerson: "NewName");
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
        public async Task UpdateAsync_Should_Replace_Details()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(detailCount: 2), 1);
            ctx.ChangeTracker.Clear();

            var updated = BuildEntity(detailCount: 4);
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

            var ghost = BuildEntity();
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(), 1);
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
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity(), 1);
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
