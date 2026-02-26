using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DispatchAddressMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DispatchAddressMaster
{
    /// <summary>
    /// Integration tests for DispatchAddressMasterCommandRepository.
    /// Verifies EF Core Create, Update, and SoftDelete operations.
    /// City/State/Country are cross-module FKs — not physically enforced.
    /// DispatchAddressMaster table is cleared before each test.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class DispatchAddressMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;

        public DispatchAddressMasterCommandRepositoryTests(DbFixture fixture)
        {
            _fixture = fixture;
        }

        private DispatchAddressMasterCommandRepository CreateRepository(ApplicationDbContext ctx)
            => new(ctx);

        private static Domain.Entities.DispatchAddressMaster BuildEntity(
            string name = "Test Dispatch Address",
            string addressLine1 = "123 Main Street",
            int cityId = 1,
            int stateId = 1,
            int countryId = 1,
            string pinCode = "110001") =>
            new()
            {
                DispatchAddressName = name,
                AddressLine1 = addressLine1,
                CityId = cityId,
                StateId = stateId,
                CountryId = countryId,
                PinCode = pinCode,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearTableAsync(ApplicationDbContext ctx)
        {
            ctx.DispatchAddressMaster.RemoveRange(ctx.DispatchAddressMaster);
            await ctx.SaveChangesAsync();
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

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

            var newId = await CreateRepository(ctx).CreateAsync(
                BuildEntity(
                    name: "Alpha Dispatch",
                    addressLine1: "456 Alpha Road",
                    cityId: 2,
                    stateId: 3,
                    countryId: 1,
                    pinCode: "500001"));
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAddressMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved.Should().NotBeNull();
            saved!.DispatchAddressName.Should().Be("Alpha Dispatch");
            saved.AddressLine1.Should().Be("456 Alpha Road");
            saved.CityId.Should().Be(2);
            saved.StateId.Should().Be(3);
            saved.CountryId.Should().Be(1);
            saved.PinCode.Should().Be("500001");
            saved.IsActive.Should().Be(Status.Active);
            saved.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public async Task CreateAsync_Should_Populate_Audit_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var newId = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAddressMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.CreatedBy.Should().Be(1);
            saved.CreatedByName.Should().Be("test-user");
            saved.CreatedIP.Should().Be("127.0.0.1");
            saved.CreatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_OptionalFields_WhenProvided()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);

            var entity = BuildEntity();
            entity.AddressLine2 = "Suite 200";
            entity.ContactPerson = "John Doe";
            entity.MobileNumber = "9876543210";
            entity.Email = "john@gmail.com";
            entity.GSTIN = "22AAAAA1234A1Z5";
            entity.Latitude = 28.6139m;
            entity.Longitude = 77.2090m;

            var newId = await CreateRepository(ctx).CreateAsync(entity);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAddressMaster.FirstOrDefaultAsync(x => x.Id == newId);

            saved!.AddressLine2.Should().Be("Suite 200");
            saved.ContactPerson.Should().Be("John Doe");
            saved.MobileNumber.Should().Be("9876543210");
            saved.GSTIN.Should().Be("22AAAAA1234A1Z5");
            saved.Latitude.Should().Be(28.6139m);
            saved.Longitude.Should().Be(77.2090m);
        }

        // ── UpdateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_Should_Return_UpdatedId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("Original Address", "1 Old Street"));
            ctx.ChangeTracker.Clear();

            var updatedId = await CreateRepository(ctx).UpdateAsync(new Domain.Entities.DispatchAddressMaster
            {
                Id = id,
                DispatchAddressName = "Updated Address",
                AddressLine1 = "2 New Street",
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                PinCode = "110001",
                IsActive = Status.Active
            });

            updatedId.Should().Be(id);
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Mutable_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity("Original Name", "Old Street", pinCode: "110001"));
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Domain.Entities.DispatchAddressMaster
            {
                Id = id,
                DispatchAddressName = "Updated Name",
                AddressLine1 = "New Street",
                AddressLine2 = "Floor 5",
                CityId = 2,
                StateId = 2,
                CountryId = 2,
                PinCode = "400001",
                ContactPerson = "Jane",
                MobileNumber = "9998887776",
                IsActive = Status.Inactive
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAddressMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.DispatchAddressName.Should().Be("Updated Name");
            saved.AddressLine1.Should().Be("New Street");
            saved.AddressLine2.Should().Be("Floor 5");
            saved.CityId.Should().Be(2);
            saved.StateId.Should().Be(2);
            saved.CountryId.Should().Be(2);
            saved.PinCode.Should().Be("400001");
            saved.ContactPerson.Should().Be("Jane");
            saved.MobileNumber.Should().Be("9998887776");
            saved.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).UpdateAsync(new Domain.Entities.DispatchAddressMaster
            {
                Id = 99999,
                DispatchAddressName = "Ghost",
                AddressLine1 = "Ghost Street",
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                PinCode = "110001",
                IsActive = Status.Active
            });

            result.Should().Be(0);
        }

        [Fact]
        public async Task UpdateAsync_Should_Populate_ModifiedAuditFields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).UpdateAsync(new Domain.Entities.DispatchAddressMaster
            {
                Id = id,
                DispatchAddressName = "Modified Address",
                AddressLine1 = "Modified Street",
                CityId = 1,
                StateId = 1,
                CountryId = 1,
                PinCode = "110001",
                IsActive = Status.Active
            });
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAddressMaster.FirstOrDefaultAsync(x => x.Id == id);
            saved!.ModifiedBy.Should().Be(1);
            saved.ModifiedByName.Should().Be("test-user");
            saved.ModifiedIP.Should().Be("127.0.0.1");
            saved.ModifiedDate.Should().NotBeNull();
        }

        // ── SoftDeleteAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_WhenEntityExists()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_SetIsDeleted_Flag()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var saved = await ctx.DispatchAddressMaster
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            saved!.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenEntityNotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var result = await CreateRepository(ctx).SoftDeleteAsync(99999, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_False_WhenAlreadyDeleted()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearTableAsync(ctx);
            var id = await CreateRepository(ctx).CreateAsync(BuildEntity());
            ctx.ChangeTracker.Clear();

            await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var result = await CreateRepository(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeFalse();
        }
    }
}
