using Microsoft.EntityFrameworkCore;
using SalesManagement.Domain.Entities;
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

        public SalesEnquiryCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesEnquiryCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new SalesEnquiryCommandRepository(ctx);

        private SalesEnquiryHeader BuildHeader(
            int partyId = 1,
            string? contactPerson = "Test Contact",
            string? remarks = "Test Enquiry",
            int? salesLeadId = null)
            => new SalesEnquiryHeader
            {
                PartyId = partyId,
                EnquiryDate = DateTimeOffset.UtcNow,
                ContactPerson = contactPerson,
                ExpectedDeliveryDate = DateTimeOffset.UtcNow.AddDays(30),
                PaymentTermId = null,
                SalesLeadId = salesLeadId,
                Remarks = remarks,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.SalesEnquiryDetail");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.SalesEnquiryHeader");
        }

        // ── CreateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildHeader());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Header_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var header = BuildHeader(partyId: 5, contactPerson: "Test Person", remarks: "Important Enquiry");
            var newId = await CreateRepository(ctx).CreateAsync(header);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesEnquiryHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.PartyId.Should().Be(5);
            saved.ContactPerson.Should().Be("Test Person");
            saved.Remarks.Should().Be("Important Enquiry");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildHeader());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesEnquiryHeader.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_With_Detail_Lines()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var header = BuildHeader();
            header.SalesEnquiryDetails = new List<SalesEnquiryDetail>
            {
                new SalesEnquiryDetail { ItemId = 1, Quantity = 10, ExmillRate = 100m, TargetPrice = 95m }
            };

            var newId = await CreateRepository(ctx).CreateAsync(header);
            ctx.ChangeTracker.Clear();

            var detailCount = await ctx.SalesEnquiryDetail
                .CountAsync(d => d.SalesEnquiryHeaderId == newId);

            detailCount.Should().Be(1);
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Persist_Header_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildHeader(partyId: 1, contactPerson: "Original Person"));
            ctx.ChangeTracker.Clear();

            var updated = BuildHeader(partyId: 7, contactPerson: "Updated Person", remarks: "Updated Remarks");
            updated.Id = id;

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);
            var saved = await ctx.SalesEnquiryHeader.FirstOrDefaultAsync(x => x.Id == id);
            saved!.PartyId.Should().Be(7);
            saved.ContactPerson.Should().Be("Updated Person");
            saved.Remarks.Should().Be("Updated Remarks");
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var updated = BuildHeader();
            updated.Id = 99999;

            var resultId = await CreateRepository(ctx).UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Replace_Detail_Lines()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var header = BuildHeader();
            header.SalesEnquiryDetails = new List<SalesEnquiryDetail>
            {
                new SalesEnquiryDetail { ItemId = 1, Quantity = 5, ExmillRate = 50m }
            };
            var id = await repo.CreateAsync(header);
            ctx.ChangeTracker.Clear();

            var updated = BuildHeader();
            updated.Id = id;
            updated.SalesEnquiryDetails = new List<SalesEnquiryDetail>
            {
                new SalesEnquiryDetail { ItemId = 2, Quantity = 10, ExmillRate = 100m },
                new SalesEnquiryDetail { ItemId = 3, Quantity = 20, ExmillRate = 200m }
            };

            await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            var detailCount = await ctx.SalesEnquiryDetail.CountAsync(d => d.SalesEnquiryHeaderId == id);
            detailCount.Should().Be(2);
        }

        // ── SoftDeleteAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildHeader());
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Set_IsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildHeader());
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesEnquiryHeader
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
