using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.SalesContact;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.SalesContact
{
    [Collection("DatabaseCollection")]
    public sealed class SalesContactCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public SalesContactCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private SalesContactCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new SalesContactCommandRepository(ctx);

        private Domain.Entities.SalesContact BuildEntity(
            string contactName = "John Doe",
            string mobileNumber = "9876543210",
            int contactTypeId = 1,
            int? partyId = null,
            string? email = "john@test.com",
            string? remarks = "Test Contact")
            => new Domain.Entities.SalesContact
            {
                ContactName = contactName,
                MobileNumber = mobileNumber,
                ContactTypeId = contactTypeId,
                PartyId = partyId,
                Email = email,
                Remarks = remarks,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            // SalesLead references SalesContact via ContactId — clear dependents first
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.SalesLead");
            await ctx.Database.ExecuteSqlRawAsync("DELETE FROM Sales.SalesContact");
        }

        // ── CreateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_Should_Return_NewId_GreaterThanZero()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());

            newId.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields_Correctly()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity(
                contactName: "Jane Smith",
                mobileNumber: "9123456789",
                contactTypeId: 2,
                partyId: 5,
                email: "jane@test.com",
                remarks: "Important Contact");
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesContact.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.ContactName.Should().Be("Jane Smith");
            saved.MobileNumber.Should().Be("9123456789");
            saved.ContactTypeId.Should().Be(2);
            saved.PartyId.Should().Be(5);
            saved.Email.Should().Be("jane@test.com");
            saved.Remarks.Should().Be("Important Contact");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_WithNullOptionalFields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity(partyId: null, email: null, remarks: null);
            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesContact.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.PartyId.Should().BeNull();
            saved.Email.Should().BeNull();
            saved.Remarks.Should().BeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesContact.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        // ── UpdateAsync ──────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity("Original Name", "9111111111"));
            ctx.ChangeTracker.Clear();

            var updated = new Domain.Entities.SalesContact
            {
                Id = id,
                ContactName = "Updated Name",
                MobileNumber = "9222222222",
                ContactTypeId = 3,
                PartyId = 10,
                Email = "updated@test.com",
                Remarks = "Updated Remarks",
                IsActive = Status.Inactive
            };

            var resultId = await repo.UpdateAsync(updated);
            ctx.ChangeTracker.Clear();

            resultId.Should().Be(id);
            var saved = await ctx.SalesContact.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ContactName.Should().Be("Updated Name");
            saved.MobileNumber.Should().Be("9222222222");
            saved.ContactTypeId.Should().Be(3);
            saved.PartyId.Should().Be(10);
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var updated = new Domain.Entities.SalesContact
            {
                Id = 99999,
                ContactName = "Ghost",
                MobileNumber = "9000000000",
                ContactTypeId = 1,
                IsActive = Status.Active
            };

            var resultId = await CreateRepository(ctx).UpdateAsync(updated);

            resultId.Should().Be(0);
        }

        // ── SoftDeleteAsync ──────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity());
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
            var id = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.SalesContact
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

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var repo = CreateRepository(ctx);
            var id = await repo.CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await repo.SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await repo.SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
