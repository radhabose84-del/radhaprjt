using Microsoft.EntityFrameworkCore;
using SalesManagement.Infrastructure.Data;
using SalesManagement.Infrastructure.Repositories.DispatchAddressMaster;
using SalesManagement.IntegrationTests.Common;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.IntegrationTests.Repositories.DispatchAddressMaster
{
    [Collection("DatabaseCollection")]
    public sealed class DispatchAddressMasterCommandRepositoryTests
    {
        private readonly DbFixture _fixture;
        public DispatchAddressMasterCommandRepositoryTests(DbFixture fixture) => _fixture = fixture;

        private DispatchAddressMasterCommandRepository CreateRepo(ApplicationDbContext ctx) => new(ctx);

        private static SalesManagement.Domain.Entities.DispatchAddressMaster BuildEntity(string name = "DAM1") =>
            new()
            {
                DispatchAddressName = name,
                AddressLine1 = "Line 1",
                AddressLine2 = "Line 2",
                CityId = 1, StateId = 1, CountryId = 1,
                PinCode = "560001",
                ContactPerson = "John",
                MobileNumber = "9876543210",
                Email = "x@y.com",
                GSTIN = "29ABCDE1234F1Z5",
                Latitude = 12.97m, Longitude = 77.59m,
                FreightId = 1,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

        private async Task ClearAsync(ApplicationDbContext ctx) =>
            await _fixture.ClearAllTablesAsync();

        [Fact]
        public async Task CreateAsync_Should_Return_NewId()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DAM_C1"));

            id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateAsync_Should_Persist_Fields()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);

            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DAM_C2"));
            ctx.ChangeTracker.Clear();
            var saved = await ctx.DispatchAddressMaster.FirstAsync(x => x.Id == id);

            saved.DispatchAddressName.Should().Be("DAM_C2");
            saved.PinCode.Should().Be("560001");
            saved.GSTIN.Should().Be("29ABCDE1234F1Z5");
        }

        [Fact]
        public async Task UpdateAsync_Should_Persist_Changes()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DAM_U1"));
            ctx.ChangeTracker.Clear();

            var entity = BuildEntity("DAM_U1_New");
            entity.Id = id;
            entity.AddressLine1 = "Updated Line";
            entity.IsActive = Status.Inactive;

            var result = await CreateRepo(ctx).UpdateAsync(entity);
            ctx.ChangeTracker.Clear();

            result.Should().Be(id);
            var reloaded = await ctx.DispatchAddressMaster.FirstAsync(x => x.Id == id);
            reloaded.DispatchAddressName.Should().Be("DAM_U1_New");
            reloaded.AddressLine1.Should().Be("Updated Line");
            reloaded.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Zero_When_NotFound()
        {
            await using var ctx = _fixture.CreateFreshDbContext();

            var ghost = BuildEntity("GH");
            ghost.Id = 9999999;

            var result = await CreateRepo(ctx).UpdateAsync(ghost);

            result.Should().Be(0);
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Return_True_When_Successful()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DAM_D1"));
            ctx.ChangeTracker.Clear();

            var result = await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task SoftDeleteAsync_Should_Flag_Record()
        {
            await using var ctx = _fixture.CreateFreshDbContext();
            await ClearAsync(ctx);
            var id = await CreateRepo(ctx).CreateAsync(BuildEntity("DAM_D2"));
            ctx.ChangeTracker.Clear();

            await CreateRepo(ctx).SoftDeleteAsync(id, CancellationToken.None);
            ctx.ChangeTracker.Clear();

            var reloaded = await ctx.DispatchAddressMaster.FirstAsync(x => x.Id == id);
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
